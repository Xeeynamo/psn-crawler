using Newtonsoft.Json;
using psncrawler.Playstation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace psncrawler
{
    public interface ICrawlerNotifier
    {
        Task NotifyNewGameAsync(Tmdb2 database);
        Task NotifyUpdateAsync(TitlePatch patch);
    }

    public class Crawler
    {
        private class CrawlerException : Exception
        {
            private readonly Exception innerException;

            public CrawlerException(Exception innerException, Title title) :
                base($"Crawler error on {title}: {innerException.Message}", innerException)
            {
                this.innerException = innerException;
            }

            public override string Source => innerException.Source;
            public override string StackTrace => innerException.StackTrace;
        }

        private const int MaxTitleId = 100000;
        private readonly ILogger _logger;
        private readonly ICrawlerNotifier _notifier;
        private readonly string _basePath;
        private readonly int _threadCount;

        public Crawler(
            ILogger logger, ICrawlerNotifier notifier, string basePath, int threadCount)
        {
            _logger = logger;
            _notifier = notifier;
            _basePath = basePath;
            _threadCount = threadCount;
        }

        public async Task ExploreMultithred(string prefix, int idStart, int idEnd, string environment)
        {
            var increment = _threadCount * 50;

            for (var i = idStart; i < idEnd; i += increment)
            {
                await ExploreMultithreadInternal(prefix, i, i + increment, environment);
            }
        }

        private async Task ExploreMultithreadInternal(string prefix, int idStart, int idEnd, string environment)
        {
            await _logger.DebugAsync($"Exploring from {prefix}{idStart:D05} to {prefix}{idEnd:D05} for {environment}...");

            int titleCount = idEnd - idStart;
            var titlePerThread = titleCount / _threadCount;

            var taskList = new List<Task>();
            for (int i = 0; i < _threadCount - 1; i++)
            {
                var newIdEnd = Math.Min(idStart + titlePerThread, MaxTitleId);
                taskList.Add(Explore(prefix, idStart, newIdEnd, environment));
                idStart = newIdEnd;
            }

            taskList.Add(Explore(prefix, idStart, idEnd, environment));

            await Task.WhenAll(taskList);
        }

        private async Task Explore(string prefix, int idStart, int idEnd, string environment)
        {
            for (int i = idStart; i <= idEnd; i++)
            {
                await TryFind(new Title($"{prefix}{i:D05}"), environment);
            }
        }

        private async Task TryFind(Title titleId, string environment)
        {
            try
            {
                if (AlreadyFound(titleId))
                {
                    await FindAndWriteTitleUpdate(titleId, environment);
                }
                else
                {
                    await FindAndWriteTitleMetadata(titleId, environment);
                    await FindAndWriteTitleUpdate(titleId, environment);
                }
            }
            catch (AggregateException e)
            {
                var psnException = (e.InnerException as PsnException);
                if (psnException != null)
                {
                    if (ShouldRetry(psnException))
                        await TryFind(titleId, environment);
                }
                else
                    await _logger.ExceptionAsync(new CrawlerException(e.InnerException, titleId));
            }
            catch (PsnException e)
            {
                if (ShouldRetry(e))
                    await TryFind(titleId, environment);
            }
            catch (Exception e)
            {
                await _logger.ExceptionAsync(new CrawlerException(e, titleId));
            }
        }

        private async Task FindAndWriteTitleMetadata(Title title, string environment)
        {
            var content = await Psn.GetTmdb(title, environment);
            var tmdb = AsTmdb(content);

            await _logger.InfoAsync($"{title} found!");

            var titlePath = GetTitlePath(title);
            Directory.CreateDirectory(titlePath);

            if (content[0] == '{')
                await File.WriteAllTextAsync($"{titlePath}/{title}_00.json", content);
            else if (content[0] == '<')
                await File.WriteAllTextAsync($"{titlePath}/{title}_00.xml", content);
            else
            {
                await _logger.WarningAsync($"Tmdb of {title} not recognized. Starts with '{content[0]}'");
                await File.WriteAllTextAsync($"{titlePath}/{title}_00.raw", content);
            }

            await _notifier.NotifyNewGameAsync(tmdb);
        }

        private async Task FindAndWriteTitleUpdate(Title title, string environment)
        {
            var titlePath = GetTitlePath(title);
            var content = await Psn.GetUpdate(title, environment);

            if (string.IsNullOrEmpty(content))
                return;

            var titlePatch = AsTitlePatch(content);
            var version = NormalizePatchVersion(titlePatch);

            var updateFilePath = $"{titlePath}/{title}-ver-{version}.xml";
            if (!File.Exists(updateFilePath))
            {
                await _logger.InfoAsync($"Update {version} found for {title}");

                await File.WriteAllTextAsync(updateFilePath, content);

                await _notifier.NotifyUpdateAsync(titlePatch);
            }

        }

        private bool AlreadyFound(Title title) => Directory.Exists(GetTitlePath(title));

        private bool ShouldRetry(PsnException e) => e.StatusCode != 404;

        private string GetTitlePath(Title title) => GetFilePath(title.ToString());
        private string GetFilePath(string fileName) => Path.Combine(_basePath, fileName);


        private static Tmdb2 AsTmdb(string content)
        {
            if (content.Length == 0)
                return null;

            if (content[0] == '{')
                return JsonConvert.DeserializeObject<Tmdb2>(content);

            using var reader = new StringReader(content);
            var tmdb = new XmlSerializer(typeof(TmdbTitleInfo)).Deserialize(reader) as TmdbTitleInfo;

            return new Tmdb2
            {
                npTitleId = tmdb.Id,
                console = tmdb.Console,
                names = new List<Tmdb2Name>()
                {
                    new Tmdb2Name
                    {
                        Name = tmdb.Name
                    }
                },
                parentalLevel = int.Parse(tmdb.Parentallevel),
                icons = new List<Tmdb2Icon>()
                {
                    new Tmdb2Icon
                    {
                        Type = tmdb.Icon.Type,
                        Icon = tmdb.Icon.Url
                    }
                }
            };
        }

        private static TitlePatch AsTitlePatch(string content)
        {
            using var reader = new StringReader(content);
            return new XmlSerializer(typeof(TitlePatch)).Deserialize(reader) as TitlePatch;
        }

        private static string NormalizePatchVersion(TitlePatch titlePatch) =>
            titlePatch.Tag.Package.Version.Replace(".", "");
    }
}
