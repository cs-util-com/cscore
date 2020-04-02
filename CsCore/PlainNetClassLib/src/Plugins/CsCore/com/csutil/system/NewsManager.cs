using System;

namespace com.csutil.system {

    public class NewsManagerTests {

    }

    public class News {

        public string title { get; set; }
        public string date { get; set; }
        public string type { get; set; }
        public string color { get; set; }
        public string description { get; set; }
        public string thumbnailUrl { get; set; }
        public string imageUrl { get; set; }
        public string detailsUrl { get; set; }
        public string detailsUrlText { get; set; }

        public enum NewsType {
            Blog, Announcement, ComingSoon, Beta, New, Improvement, Warning, Fix, Unknown
        }

        public NewsType GetNewsType() { return EnumHelper.TryParse(type, NewsType.Unknown); }

    }

}