using Newtonsoft.Json;
using psncrawler.Playstation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace psncrawler
{
    class Program
    {
        private const string BasePath = "/data/psndb/psndb";
        private const string LogFile = "/data/psndb/log";
        private static readonly TimeSpan delay = TimeSpan.FromHours(4);

        private static readonly string[] Letters = new string[]
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
            "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
            "U", "V", "W", "X", "Y", "Z"
        };

        private static readonly string[] ProductPrefixes = new string[]
        {
            "CUS",
            "PCS",
            "NPE",
            "NPU",
            "NPJ",
            "NPH",
            "NPS",
            "BLE",
            "BLU",
            "BLJ",
            "BLH",
            "BLS",
            "ULE",
            "UCE",
            "ULU",
            "UCU",
            "ULJ",
            "UCJ",
            "ULH",
            "UCH",
            "ULs",
            "UCs",
            "SLE",
            "SCE",
            "SLU",
            "SCU",
            "SLJ",
            "SCJ",
            "SLH",
            "SCH",
            "SLS",
            "SCS",
        }.SelectMany(x => Letters.Select(ch => x + ch)).ToArray();

        static async Task Main(string[] args)
        {
            var configuration = await ReadConfiguration("secrets.json");

            var logger = SetupLogger();
            await logger.InfoAsync("Ready!");

            var notifier = await SetupNotifier(configuration, logger);

            if (!Directory.Exists(BasePath))
            {
                await logger.WarningAsync($"Base path {BasePath} did not exist");
                return;
            }

            while (true)
            {
                foreach (var prefix in ProductPrefixes)
                {
                    await new Crawler(logger, notifier, BasePath, 10).ExploreMultithred(prefix, 0, 99999, "np");
                }
                await Task.Delay(delay);
            }
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
                //new ConcurrencyLogger(new ActionLogger(LogOnFile))
            });

        private static async Task<ICrawlerNotifier> SetupNotifier(
            Configuration configuration, ILogger logger)
        {
            //var notifier = await TwitterCrawlerNotifier.FromConsumer(
            //    configuration.TwitterApiKey,
            //    configuration.TwitterApiSecret,
            //    () =>
            //    {
            //        Console.Write("Twitter pin: ");
            //        return Console.ReadLine();
            //    });

            return new DummyCrawlerNotifier();

            var notifier = await TwitterCrawlerNotifier.FromAccess(
                configuration.TwitterApiKey,
                configuration.TwitterApiSecret,
                configuration.TwitterToken,
                configuration.TwitterTokenSecret);
            notifier.Logger = logger;

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
        public Task NotifyNewGameAsync(Tmdb2 database) => Task.CompletedTask;

        public Task NotifyUpdateAsync(TitlePatch patch) => Task.CompletedTask;
    }

    internal static class DateTimeExtensions
    {
        public static string ToIso8601(this DateTime dateTime) =>
            dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
