using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using psncrawler.Playstation;

namespace psncrawler
{
    class Program
    {
        private static readonly object _logger = new object();

        static async Task Main(string[] args)
        {
            const int increment = 3000;
            const string environment = "np";

            for (int i = 7000; i < 100000; i += increment)
            {
                await ExploreMultithread(i, i + increment, environment);
            }
        }

        public static async Task ExploreMultithread(int idStart, int idEnd, string environment)
        {
            Info($"Exploring from {idStart:D05} to {idEnd:D05}...");

            const int threadCount = 30;
            int titleCount = idEnd - idStart;
            var titlePerThread = titleCount / threadCount;

            var taskList = new List<Task>();
            for (int i = 0; i < threadCount - 1; i++)
            {
                var newIdEnd = idStart + titlePerThread;
                taskList.Add(Explore(idStart, newIdEnd, environment));
                idStart = newIdEnd;
            }
            
            taskList.Add(Explore(idStart, idEnd, environment));

            await Task.WhenAll(taskList);
        }

        public static async Task Explore(int idStart, int idEnd, string environment)
        {
            for (int i = idStart; i <= idEnd; i++)
                await TryFind(i, environment);
        }

        public static async Task TryFind(int id, string environment)
        {
            if (AlreadyFound(id))
            {
                try
                {
                    var titlePath = GetTitlePath(id);
                    var update = await Psn.GetUpdate(new Title(Cusa(id)));

                    using var reader = new StringReader(update);
                    var content = new XmlSerializer(typeof(TitlePatch)).Deserialize(reader) as TitlePatch;
                    var version = content.Tag.Package.Version.Replace(".", "");

                    using var file = File.CreateText($"{titlePath}/{Cusa(id)}-ver-{version}.xml");
                    file.WriteLine(update);
                }
                catch {}
            }
            
            return;

            try
            {
                var tmdb = await Psn.GetTmdb(new Title(Cusa(id)));
                var content = JsonConvert.DeserializeObject<Tmdb>(tmdb);
                Info($"{content.contentId} found!");

                var titlePath = GetTitlePath(id);
                Directory.CreateDirectory(titlePath);

                using var file = File.CreateText($"{titlePath}/{Cusa(id)}_00.json");
                file.WriteLine(tmdb);
            }
            catch (AggregateException e)
            {
                var psnException = (e.InnerException as PsnException);
                if (psnException != null)
                {
                    if (ShouldRetry(psnException))
                        await TryFind(id, environment);
                }
                else
                    Error(e.InnerException, id);
                
            }
            catch (PsnException e)
            {
                if (ShouldRetry(e))
                    await TryFind(id, environment);
            }
            catch (Exception e)
            {
                Error(e, id);
            }
        }

        private static bool AlreadyFound(int id) => Directory.Exists(GetTitlePath(id));

        private static bool ShouldRetry(PsnException e)
        {
            if (e.StatusCode != 404)
                Warning(e.Message);

            return e.StatusCode != 404;
        }

        private static void Info(string message) => LogInternal($"INF {message}");
        private static void Warning(string message) => LogInternal($"WRN {message}");
        private static void Error(string message) => LogInternal($"ERR {message}");
        private static void Error(Exception ex, int id) => Error($"Id {id:D05}: {ex.Message}");

        private static void LogInternal(string message)
        {
            lock(_logger)
            {
                var logMessage = $"[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}] {message}";
                Console.WriteLine(logMessage);
                using var logger = File.AppendText(GetFilePath("log"));
                logger.WriteLine(logMessage);
            }
        }

        private static string Cusa(int titleId) => $"CUSA{titleId:D05}";
        private static string GetTitlePath(int titleId) => GetFilePath(Cusa(titleId));
        private static string GetFilePath(string fileName) => $"D:/_psn/{fileName}";
    }
}
