using psncrawler.Playstation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TweetSharp;

namespace psncrawler
{
    public class TwitterException : Exception
    {
        public TwitterException(TwitterResponse response) :
            base($"Twitter error: {response.Error}")
        { }
    }

    public class TwitterCrawlerNotifier : ICrawlerNotifier
    {
        private readonly TwitterService _twitterService;

        public ILogger Logger { get; set; }

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

        public async Task NotifyNewGameAsync(Tmdb2 database)
        {
            var message = SocialMessageService.GetNewGameMessage(database);
            if (message == null)
                return;

            var mediaIds = new List<string>();

            if (!string.IsNullOrEmpty(database.backgroundImage))
            {
                using var client = new HttpClient();
                using var response = await client.GetAsync(database.backgroundImage);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();

                    var uploadResponse = await _twitterService.UploadMediaAsync(new UploadMediaOptions
                    {
                        Media = new MediaFile
                        {
                            FileName = $"{database.contentId}.png",
                            Content = stream
                        }
                    });

                    if (uploadResponse.Response.StatusCode != HttpStatusCode.OK)
                    {
                        await Logger.ErrorAsync($"Tweet media for {database?.contentId ?? "unk"} failed to upload: {uploadResponse.Response.Error}");
                    }
                    else
                    {
                        if (uploadResponse?.Value?.Media_Id != null)
                        {
                            await Logger.DebugAsync($"Tweet media for {database?.contentId ?? "unk"} uploaded to {uploadResponse.Value.Media_Id}");
                            mediaIds.Add(uploadResponse.Value.Media_Id);
                        }

                    }
                }
                else
                    await Logger.ErrorAsync($"Unable to download {nameof(Tmdb2.backgroundImage)} for {database?.contentId ?? "unk"}");
            }

            var twitterResponse = await _twitterService.SendTweetAsync(new SendTweetOptions
            {
                Status = message,
                MediaIds = mediaIds
            });

            if (twitterResponse.Response.StatusCode != HttpStatusCode.OK)
            {
                await Logger.ErrorAsync($"Tweet update for {database?.contentId ?? "unk"} failed: {twitterResponse.Response.Error}");
                throw new TwitterException(twitterResponse.Response);
            }

            await Logger.DebugAsync($"Tweet new game title {database?.contentId ?? "unk"}");
        }

        public async Task NotifyUpdateAsync(TitlePatch patch)
        {
            var name = patch.Tag.Package.Paramsfo.Title;
            var versionString = patch.Tag.Package.Version;
            var versionArray = versionString.Split('.');
            var version = $"{int.Parse(versionArray[0])}.{versionArray[1]}";

            var twitterResponse = await _twitterService.SendTweetAsync(new SendTweetOptions
            {
                Status = $"The update {version} for {name} has been released!"
            });

            if (twitterResponse.Response.StatusCode != HttpStatusCode.OK)
            {
                await Logger.ErrorAsync($"Tweet update for {patch.Titleid} failed: {twitterResponse.Response.Error}");
                throw new TwitterException(twitterResponse.Response);
            }

            await Logger.DebugAsync($"Post tweet update for {patch.Titleid}");
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
