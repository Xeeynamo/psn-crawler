﻿using System;
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
            var logger = SetupLogger();
            await logger.InfoAsync("Ready!");

            if (!Directory.Exists(BasePath))
            {
                await logger.WarningAsync($"Base path {BasePath} did not exist");
                Directory.CreateDirectory(BasePath);
            }

            await new Crawler(logger, BasePath, 30).ExploreMultithred(0, 30000, "np");
        }

        private static ILogger SetupLogger() =>
            new Logger(new List<ILoggerHandler>()
            {
                new ConcurrencyLogger(new ActionLogger(LogOnConsole)),
                new ConcurrencyLogger(new ActionLogger(LogOnFile))
            });

        private static Task LogOnConsole(int severity, DateTime dateTime, string message)
        {
            Console.WriteLine($"[{dateTime.ToIso8601()}] {message}");
            return Task.CompletedTask;
        }

        private static Task LogOnFile(int severity, DateTime dateTime, string message) =>
            File.AppendAllTextAsync(LogFile, $"[{dateTime.ToIso8601()}] {message}\n");
    }

    internal static class DateTimeExtensions
    {
        public static string ToIso8601(this DateTime dateTime) =>
            dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
