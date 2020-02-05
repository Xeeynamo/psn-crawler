using System.Xml.Serialization;

namespace psncrawler.Playstation
{
    [XmlRoot(ElementName = "delta_info_set")]
    public class PatchDeltaInfoSet
    {
        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; }
    }

    [XmlRoot(ElementName = "paramsfo")]
    public class PatchParamSfo
    {
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "title_01")]
        public string Title_01 { get; set; }
        [XmlElement(ElementName = "title_02")]
        public string Title_02 { get; set; }
        [XmlElement(ElementName = "title_03")]
        public string Title_03 { get; set; }
        [XmlElement(ElementName = "title_04")]
        public string Title_04 { get; set; }
        [XmlElement(ElementName = "title_05")]
        public string Title_05 { get; set; }
    }

    [XmlRoot(ElementName = "package")]
    public class PatchPackage
    {
        [XmlElement(ElementName = "delta_info_set")]
        public PatchDeltaInfoSet Delta_info_set { get; set; }
        [XmlElement(ElementName = "paramsfo")]
        public PatchParamSfo Paramsfo { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
        [XmlAttribute(AttributeName = "digest")]
        public string Digest { get; set; }
        [XmlAttribute(AttributeName = "manifest_url")]
        public string Manifest_url { get; set; }
        [XmlAttribute(AttributeName = "content_id")]
        public string Content_id { get; set; }
        [XmlAttribute(AttributeName = "system_ver")]
        public string System_ver { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "remaster")]
        public string Remaster { get; set; }
        [XmlAttribute(AttributeName = "patchgo")]
        public string Patchgo { get; set; }
    }

    [XmlRoot(ElementName = "latest_playgo_manifest")]
    public class PatchLatestPlaygoManifest
    {
        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; }
    }

    [XmlRoot(ElementName = "tag")]
    public class PatchTag
    {
        [XmlElement(ElementName = "package")]
        public PatchPackage Package { get; set; }
        [XmlElement(ElementName = "latest_playgo_manifest")]
        public PatchLatestPlaygoManifest Latest_playgo_manifest { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "mandatory")]
        public string Mandatory { get; set; }
    }

    [XmlRoot(ElementName = "titlepatch")]
    public class TitlePatch
    {
        [XmlElement(ElementName = "tag")]
        public PatchTag Tag { get; set; }
        [XmlAttribute(AttributeName = "titleid")]
        public string Titleid { get; set; }
    }

}