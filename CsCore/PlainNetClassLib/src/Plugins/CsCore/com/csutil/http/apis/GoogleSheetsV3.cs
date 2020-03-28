using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace com.csutil.http.apis {

    [Obsolete("The Google Sheets v3 API will be shut down on September 30, 2020, see https://developers.google.com/sheets/api/v3/data")]
    public static class GoogleSheetsV3 {

        // Only works if the sheet is published (not the same as setting it to public)
        public static async Task<List<List<string>>> GetSheet(string sheetId) {
            return GetSheetsFromAtomResp(await AtomFeed.Get(GetApiUrlFor(sheetId)));
        }

        public static Uri GetShareLinkFor(string id) { return new Uri("https://docs.google.com/spreadsheets/d/" + id); }

        public static Uri GetApiUrlFor(string id) {

            return new Uri("https://spreadsheets.google.com/feeds/list/" + id + "/od6/public/values?alt=json");
        }

        public static List<List<string>> GetSheetsFromAtomResp(AtomFeed.Response result) {
            List<List<string>> sheets = new List<List<string>>();
            foreach (var columnPlusMetaInfo in result.feed.entry) {
                var columnKeys = columnPlusMetaInfo.Keys.Filter(x => x.StartsWith("gsx$"));
                var column = columnKeys.Map(key => columnPlusMetaInfo[key]);
                var columnAsStrings = column.Map(row => (row as JObject).GetValue("$t").ToString());
                sheets.Add(columnAsStrings.Filter(x => !x.IsNullOrEmpty()).ToList());
            }
            return sheets;
        }

    }

}