using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.csutil.http.apis {

    public class GoogleSheets {

        public static async Task<List<List<string>>> GetSheet(string sheetId) {
            return GetSheetFromResult(await GetFullJson(sheetId));
        }

        public static Task<GSheetsResp.Answer> GetFullJson(string sheetId) {
            var sheetsUrl = "https://spreadsheets.google.com/feeds/list/" + sheetId + "/od6/public/values?alt=json";
            return new Uri(sheetsUrl).SendGET().GetResult<GSheetsResp.Answer>();
        }

        public static List<List<string>> GetSheetFromResult(GSheetsResp.Answer result) {
            List<List<string>> sheet = new List<List<string>>();
            var columnsPlusMetaInfo = result.feed.entry;
            foreach (var columnPlusMetaInfo in columnsPlusMetaInfo) {
                var columnKeys = columnPlusMetaInfo.Keys.Filter(x => x.StartsWith("gsx$"));
                var column = columnKeys.Map(key => columnPlusMetaInfo[key]);
                var columnAsStrings = column.Map(row => (row as JObject).GetValue("$t").ToString());
                sheet.Add(columnAsStrings.ToList());
            }
            return sheet;
        }

    }

    public class GSheetsResp {
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

        public class Answer {
            public string version { get; set; }
            public string encoding { get; set; }
            public Feed feed { get; set; }
        }
    }

}