using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace psncrawler.Playstation
{
    [XmlRoot(ElementName = "icon")]
    public class TmdbIcon
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlText]
        public string Url { get; set; }
    }

    public class Tmdb2Icon
    {
        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Tmdb2Name
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lang")]
        public string Lang { get; set; }
    }

    [XmlRoot(ElementName = "title-info")]
    public class TmdbTitleInfo
    {
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        [XmlElement(ElementName = "console")]
        public string Console { get; set; }

        [XmlElement(ElementName = "media-type")]
        public string Mediatype { get; set; }

        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "parental-level")]
        public string Parentallevel { get; set; }

        [XmlElement(ElementName = "icon")]
        public TmdbIcon Icon { get; set; }

        [XmlElement(ElementName = "resolution")]
        public string Resolution { get; set; }

        [XmlElement(ElementName = "sound-format")]
        public string Soundformat { get; set; }

        [XmlAttribute(AttributeName = "rev")]
        public string Rev { get; set; }
    }

    public class Tmdb2
    {
        public int revision { get; set; }
        public int patchRevision { get; set; }
        public int formatVersion { get; set; }
        public string npTitleId { get; set; }
        public string console { get; set; }
        public List<Tmdb2Name> names { get; set; }
        public List<Tmdb2Icon> icons { get; set; }
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