using System.Collections.Generic;

namespace wow2.Modules.Voice
{
    public class YoutubeVideoMetadata
    {
        public string title { get; set; }
        public string webpage_url { get; set; }
        public string description { get; set; }

        public int? view_count { get; set; }
        public int? like_count { get; set; }
        public int? dislike_count { get; set; }

        public List<YoutubeVideoMetadataThumbnails> thumbnails { get; set; }
    }

    public class YoutubeVideoMetadataThumbnails
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string resolution { get; set; }
        public string id { get; set; }
    }
}