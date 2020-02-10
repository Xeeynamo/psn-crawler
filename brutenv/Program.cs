using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace brutenv
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var envs = new List<string>();

            foreach (var env in GetCombinations(""))
            {
                const int threadCount = 500;
                envs.Add(env);
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                if (envs.Count >= threadCount)
                {
                    await Task.WhenAll(envs.Select(CheckAndLog));
                    envs.Clear();

                    var elapsed = stopWatch.ElapsedMilliseconds;
                    var requestsPerSecond = (int)(threadCount * 1000 / elapsed);
                    Console.Write(
                        $"\r{env}, {requestsPerSecond} rps   ");
                }
            }
        }

        private static readonly char[] _allowedCharacters = new[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k',
            'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
            'w', 'x', 'y', 'z', '-', '_'
        };

        private const int MaxAllowed = 6;
        private const int MinAllowed = 5;

        private static IEnumerable<string> GetCombinations(string str)
        {
            if (str.Length > MinAllowed)
            {
                foreach (var ch in _allowedCharacters)
                {
                    yield return str + ch;
                }
            }

            if (str.Length < MaxAllowed - 1)
            {
                foreach (var ch in _allowedCharacters)
                {
                    foreach (var _ in GetCombinations(str + ch))
                        yield return _;
                }
            }
        }

        private static async Task CheckAndLog(string environment)
        {
            var result = await Check(environment);
            if (result)
            {
                Console.WriteLine(environment);
                await File.AppendAllTextAsync("psnenv.txt", $"{environment}\n");
            }
        }

        private static async Task<bool> Check(string environment) =>
            await CheckDns($"tmdb.{environment}.dl.playstation.net") ||
            await CheckDns($"gs-sec.ww.{environment}.dl.playstation.net");

        private static async Task<bool> CheckDns(string url)
        {
            try
            {
                await Dns.GetHostEntryAsync(url);
                return true;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                if (ex.Message != "No such host is known." &&
                    ex.Message != "Name or service not known")
                    Console.WriteLine(ex.Message);

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return false;
            }
        }
    }
}
