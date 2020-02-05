using Newtonsoft.Json;
using psncrawler.Playstation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace psncrawler
{
    class Program
    {
        private const string BasePath = "../../../../../psndb";
        private const string LogFile = "log";

        static async Task Main(string[] args)
        {
            var configuration = await ReadConfiguration("secrets.json");

            var logger = SetupLogger();
            await logger.InfoAsync("Ready!");

            var notifier = await SetupNotifier(configuration);

            if (!Directory.Exists(BasePath))
            {
                await logger.WarningAsync($"Base path {BasePath} did not exist");
                Directory.CreateDirectory(BasePath);
            }

            await new Crawler(logger, notifier, BasePath, 30).ExploreMultithred(0, 30000, "np");
        }

        private static async Task<Configuration> ReadConfiguration(string fileName)
        {
            var content = await File.ReadAllTextAsync(fileName);
            return JsonConvert.DeserializeObject<Configuration>(content);
        }

        private static ILogger SetupLogger() =>
            new Logger(new List<ILoggerHandler>()
            {
                new ConcurrencyLogger(new ActionLogger(LogOnConsole)),
                new ConcurrencyLogger(new ActionLogger(LogOnFile))
            });

        private static async Task<ICrawlerNotifier> SetupNotifier(Configuration configuration)
        {
            //var notifier = await TwitterCrawlerNotifier.FromConsumer(
            //    configuration.TwitterApiKey,
            //    configuration.TwitterApiSecret,
            //    () =>
            //    {
            //        Console.Write("Twitter pin: ");
            //        return Console.ReadLine();
            //    });

            var notifier = await TwitterCrawlerNotifier.FromAccess(
                configuration.TwitterApiKey,
                configuration.TwitterApiSecret,
                configuration.TwitterToken,
                configuration.TwitterTokenSecret);

            return notifier;
        }

        private static Task LogOnConsole(int severity, DateTime dateTime, string message)
        {
            Console.WriteLine($"[{dateTime.ToIso8601()}] {message}");
            return Task.CompletedTask;
        }

        private static Task LogOnFile(int severity, DateTime dateTime, string message) =>
            File.AppendAllTextAsync(LogFile, $"[{dateTime.ToIso8601()}] {message}\n");
    }

    internal class Configuration
    {
        public string TwitterApiKey { get; set; }
        public string TwitterApiSecret { get; set; }
        public string TwitterToken { get; set; }
        public string TwitterTokenSecret { get; set; }
    }

    internal class DummyCrawlerNotifier : ICrawlerNotifier
    {
        public Task NotifyNewGameAsync(Tmdb database) => Task.CompletedTask;

        public Task NotifyUpdateAsync(TitlePatch patch) => Task.CompletedTask;
    }

    internal static class DateTimeExtensions
    {
        public static string ToIso8601(this DateTime dateTime) =>
            dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
