using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.http.apis {

    public static class JsonFeed {

        /// <summary> A jsonfeed, e.g. https://hnrss.org/frontpage.jsonfeed </summary>
        public static Task<Response> Get(Uri jsonFeedUrl) { return jsonFeedUrl.SendGET().GetResult<Response>(); }

        public class Item {
            public string id { get; set; }
            public string title { get; set; }
            public string content_html { get; set; }
            public string url { get; set; }
            public string external_url { get; set; }
            public DateTime date_published { get; set; }
            public string author { get; set; }
            public string content_text { get; set; }

        }

        public class Response {
            public string version { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string home_page_url { get; set; }
            public List<Item> items { get; set; }
            public string feed_url { get; set; }
            public string icon { get; set; }
            public string favicon { get; set; }
        }

    }

}
