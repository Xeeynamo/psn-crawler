using System.Collections.Generic;

namespace psncrawler.Playstation
{
    public class TmdbName
    {
        public string name { get; set; }
        public string lang { get; set; }
    }

    public class TmdbIcon
    {
        public string icon { get; set; }
        public string type { get; set; }
    }

    public class Tmdb
    {
        public int revision { get; set; }
        public int patchRevision { get; set; }
        public int formatVersion { get; set; }
        public string npTitleId { get; set; }
        public string console { get; set; }
        public List<TmdbName> names { get; set; }
        public List<TmdbIcon> icons { get; set; }
        public int parentalLevel { get; set; }
        public string pronunciation { get; set; }
        public string contentId { get; set; }
        public string backgroundImage { get; set; }
        public string bgm { get; set; }
        public string category { get; set; }
        public int psVr { get; set; }
        public int neoEnable { get; set; }
    }
}