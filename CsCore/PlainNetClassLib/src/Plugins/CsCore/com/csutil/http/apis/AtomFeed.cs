using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace com.csutil.http {

    public static class AtomFeed {

        public static Task<Response> Get(Uri atomUrl) { return atomUrl.SendGET().GetResult<Response>(); }

        public class Id {
            [JsonProperty(PropertyName = "$t")]
            public string t { get; set; }
        }

        public class Updated {
            [JsonProperty(PropertyName = "$t")]
            public DateTime t { get; set; }
        }

        public class Category {
            public string scheme { get; set; }
            public string term { get; set; }
        }

        public class Title {
            public string type { get; set; }
            [JsonProperty(PropertyName = "$t")]
            public string t { get; set; }
        }

        public class Link {
            public string rel { get; set; }
            public string type { get; set; }
            public string href { get; set; }
        }

        public class Name {
            [JsonProperty(PropertyName = "$t")]
            public string t { get; set; }
        }

        public class Email {
            [JsonProperty(PropertyName = "$t")]
            public string t { get; set; }
        }

        public class Author {
            public Name name { get; set; }
            public Email email { get; set; }
        }

        public class OpenSearchTotalResults {
            [JsonProperty(PropertyName = "$t")]
            public string t { get; set; }
        }

        public class OpenSearchStartIndex {
            [JsonProperty(PropertyName = "$t")]
            public string t { get; set; }
        }

        public class Feed {
            public string xmlns { get; set; }
            [JsonProperty(PropertyName = "xmlns$openSearch")]
            public string xmlnsopenSearch { get; set; }
            [JsonProperty(PropertyName = "xmlns$gsx")]
            public string xmlnsgsx { get; set; }
            public Id id { get; set; }
            public Updated updated { get; set; }
            public IList<Category> category { get; set; }
            public Title title { get; set; }
            public IList<Link> link { get; set; }
            public IList<Author> author { get; set; }
            [JsonProperty(PropertyName = "openSearch$totalResults")]
            public OpenSearchTotalResults openSearchtotalResults { get; set; }
            [JsonProperty(PropertyName = "openSearch$startIndex")]
            public OpenSearchStartIndex openSearchstartIndex { get; set; }
            public IList<Dictionary<string, object>> entry { get; set; }
        }

        public class Response {
            public string version { get; set; }
            public string encoding { get; set; }
            public Feed feed { get; set; }
        }

    }

}