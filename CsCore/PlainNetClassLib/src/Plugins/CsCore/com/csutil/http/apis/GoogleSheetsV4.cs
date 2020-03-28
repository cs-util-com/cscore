using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.csutil.http.apis {

    public static class GoogleSheetsV4 {

        public static async Task<List<List<string>>> GetSheet(string apiKey, string spreadsheetId, string sheetName) {
            Response resp = await GetApiUrlFor(apiKey, spreadsheetId, sheetName).SendGET().GetResult<Response>();
            return resp.values;
        }

        /// <summary> Loads the sheet content </summary>
        /// <param name="apiKey"> Create your key at https://console.developers.google.com/apis/credentials </param>
        /// <param name="spreadsheetId"> The sheet id from the link, e.g.: https://docs.google.com/spreadsheets/d/abcd123 </param>
        public static async Task<Response> GetDocument(string apiKey, string spreadsheetId, string sheetName) {
            return await GetApiUrlFor(apiKey, spreadsheetId, sheetName).SendGET().GetResult<Response>();
        }

        public static Uri GetShareLinkFor(string spreadsheetId) {
            return new Uri("https://docs.google.com/spreadsheets/d/" + spreadsheetId);
        }

        /// <summary> See https://developers.google.com/sheets/api/reference/rest/v4/spreadsheets.values/get </summary>
        public static Uri GetApiUrlFor(string apiKey, string spreadsheetId, string sheetName) {
            return new Uri($"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{sheetName}?key={apiKey}");
        }

        public class Response {
            public string range { get; set; }
            public string majorDimension { get; set; }
            public List<List<string>> values { get; set; }
        }

    }

}