using psncrawler.Playstation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TweetSharp;

namespace psncrawler
{
    public class TwitterCrawlerNotifier : ICrawlerNotifier
    {
        private readonly TwitterService _twitterService;

        private TwitterCrawlerNotifier(TwitterService twitterService)
        {
            _twitterService = twitterService;
        }

        public Task Tweet(string message)
        {
            return _twitterService.SendTweetAsync(new SendTweetOptions
            {
                Status = message
            });
        }

        public Task NotifyNewGameAsync(Tmdb database)
        {
            return Task.CompletedTask;
        }

        public Task NotifyUpdateAsync(TitlePatch patch)
        {
            return Task.CompletedTask;
        }

        public static async Task<TwitterCrawlerNotifier> FromConsumer(
            string apiKey, string apiSecret, Func<string> getVerifyCode)
        {
            // Pass your credentials to the service
            var service = new TwitterService(apiKey, apiSecret);

            // Step 1 - Retrieve an OAuth Request Token
            var requestToken = await service.GetRequestTokenAsync();

            // Step 2 - Redirect to the OAuth Authorization URL
            var uri = service.GetAuthorizationUri(requestToken.Value);
            Process.Start(new ProcessStartInfo()
            {
                FileName = uri.ToString(),
                UseShellExecute = true
            });

            // Step 3 - Exchange the Request Token for an Access Token
            var verifier = getVerifyCode();
            var access = await service.GetAccessTokenAsync(requestToken.Value, verifier);

            // Step 4 - User authenticates using the Access Token
            service.AuthenticateWith(access.Value.Token, access.Value.TokenSecret);

            return new TwitterCrawlerNotifier(service);
        }
        
        public static Task<TwitterCrawlerNotifier> FromAccess(
            string apiKey, string apiSecret, string token, string accessToken)
        {
            var service = new TwitterService(apiKey, apiSecret, token, accessToken);
            return Task.FromResult(new TwitterCrawlerNotifier(service));
        }
    }
}
