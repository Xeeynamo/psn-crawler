using psncrawler.Playstation;
using System.Collections.Generic;
using System.Linq;

namespace psncrawler
{
    public static class SocialMessageService
    {
        public static string GetNewGameMessage(Tmdb2 database)
        {
            var name = GetDefaultName(database);
            if (name == null)
                return null;

            var region = GetRegion(database?.contentId);
            var console = GetConsoleId(database);
            var applicationId = GetApplicationId(database);
            applicationId = applicationId != null ? $"with id {applicationId}" : string.Empty;

            return $"The game {Append(name)}{Append(applicationId)}has been added to the {Append(console)}{Append(region)}PSN!";
        }

        private static string GetApplicationId(Tmdb2 database)
        {
            var npTitleId = database?.npTitleId;
            if (string.IsNullOrEmpty(npTitleId))
                return null;

            if (npTitleId.Length < 9)
                return null;

            return database.npTitleId.Substring(0, 9);
        }

        private static string GetConsoleId(Tmdb2 database)
        {
            var console = database?.console?.ToUpper();
            if (string.IsNullOrEmpty(console))
                return string.Empty;

            switch (console)
            {
                case "PS4": return "PS4";
                default: return console;
            }
        }

        private static string GetContentId(Tmdb2 database) => database?.contentId;

        private static string GetDefaultName(Tmdb2 database) =>
            database.names != null ? GetDefaultName(database.names) : null;

        private static string GetDefaultName(List<Tmdb2Name> names) =>
            names?.FirstOrDefault(IsDefaultLanguage)?.Name ??
            names?.FirstOrDefault()?.Name;

        private static bool IsDefaultLanguage(Tmdb2Name name) => string.IsNullOrEmpty(name.Lang);

        private static string GetRegion(string contentId)
        {
            var ch = contentId?.FirstOrDefault();
            if (ch == null)
                return null;

            switch (ch)
            {
                case default(char): return null;
                case 'U': return "american";
                case 'E': return "european";
                case 'J': return "japanese";
                case 'H': return "asian";
                case 'I': return "internal";
                default: return $"'{ch}'";
            }
        }

        private static string Append(string str) =>
            string.IsNullOrEmpty(str) ? string.Empty : $"{str} ";
    }
}
