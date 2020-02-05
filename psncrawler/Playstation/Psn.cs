using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace psncrawler.Playstation
{
    public class Psn
    {
        private const string TmdbHmacKey = "F5DE66D2680E255B2DF79E74F890EBF349262F618BCAE2A9ACCDEE5156CE8DF2CDF2D48C71173CDC2594465B87405D197CF1AED3B7E9671EEB56CA6753C2E6B0";
        private const string UpdateHmacKey = "AD62E37F905E06BC19593142281C112CEC0E7EC3E97EFDCAEFCDBAAFA6378D84";
        private static readonly byte[] TmdbKey = AsByteArray(TmdbHmacKey);
        private static readonly byte[] UpdateKey = AsByteArray(UpdateHmacKey);

        public static string GetTmdbUrl(Title title, string environment = "np")
        {
            switch (title.Category)
            {
                case TitleCategory.PS4: return GetTmdb2UrlInternal(title, environment);
                default: 
                    throw new NotImplementedException();
            }
        }
        
        public static string GetUpdateUrl(Title title, string environment = "np")
        {
            switch (title.Category)
            {
                case TitleCategory.PS4: return GetUpdateUrlInternal(title, environment);
                default: 
                    throw new NotImplementedException();
            }
        }

        public static async Task<string> GetTmdb(Title title) =>
            await GetContent(GetTmdbUrl(title));

        public static async Task<string> GetUpdate(Title title) => 
            await GetContent(GetUpdateUrl(title));

        private static async Task<string> GetContent(string url)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
                throw new PsnException(url, (int)response.StatusCode, body);

            return body;
        }

        private static string GetTmdb2UrlInternal(Title title, string environment)
        {
            var fullTitle = $"{title}_00";
            var hash = GetHmacSha1(TmdbKey, fullTitle);
            return $"http://tmdb.{environment}.dl.playstation.net/tmdb2/{fullTitle}_{hash}/{fullTitle}.json";
        }

        private static string GetUpdateUrlInternal(Title title, string environment)
        {
            var fullTitle = $"np_{title}";
            var hash = GetHmacSha256(UpdateKey, fullTitle).ToLower();
            return $"http://gs-sec.ww.{environment}.dl.playstation.net/plo/{environment}/{title}/{hash}/{title}-ver.xml";
        }

        private static string GetHmacSha1(byte[] key, string message)
        {
            using var sha = new HMACSHA1(key);
            sha.Initialize();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(message));

            return AsString(hash);
        }

        private static string GetHmacSha256(byte[] key, string message)
        {
            using var sha = new HMACSHA256(key);
            sha.Initialize();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(message));

            return AsString(hash);
        }

        private static byte[] AsByteArray(string hex) =>
            Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();

        private static string AsString(byte[] data) =>
            string.Join("", data.Select(x => x.ToString("X02"))); 
    }

    public class PsnException : Exception
    {
        public string Url { get; }
        public int StatusCode { get; }
        public string Body { get; }

        public PsnException(string url, int statusCode, string body) :
            base($"{url} {statusCode}:\n{body}")
            {
                Url = url;
                StatusCode = statusCode;
                Body = body;
            }
    }
}