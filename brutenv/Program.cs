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
                envs.Add(env);
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                if (envs.Count >= 200)
                {
                    await Task.WhenAll(envs.Select(CheckAndLog));
                    envs.Clear();

                    var elapsed = stopWatch.ElapsedMilliseconds;
                    Console.WriteLine($"{env} in {(int)(elapsed/1000)}.{elapsed%1000}s");
                }
            }
        }

        private static readonly char[] _allowedCharacters = new[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k',
            'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
            'w', 'x', 'y', 'z', '-', '_'
        };

        private const int MaxAllowed = 3;

        private static IEnumerable<string> GetCombinations(string str)
        {
            foreach (var ch in _allowedCharacters)
            {
                yield return str + ch;
            }

            if (str.Length < MaxAllowed)
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
                await File.AppendAllTextAsync("D:/psnenv.txt", $"{environment}\n");
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
                if (ex.Message != "No such host is known.")
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
