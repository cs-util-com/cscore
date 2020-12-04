using com.csutil.datastructures;
using com.csutil.http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zio;

namespace com.csutil {

    public static class RestRequestHelper {

        public static RestRequest WithJsonContent(this RestRequest self, object jsonContent) {
            return self.WithJsonContent(JsonWriter.GetWriter().Write(jsonContent));
        }

        public static RestRequest WithJsonContent(this RestRequest self, string jsonContent) {
            return self.WithTextContent(jsonContent, Encoding.UTF8, "application/json");
        }

        public static string ToUriEncodedString(object o) {
            var map = JsonReader.GetReader().Read<Dictionary<string, object>>(JsonWriter.GetWriter().Write(o));
            return map.Select((x) => x.Key + "=" + Uri.EscapeDataString("" + x.Value)).Aggregate((a, b) => a + "&" + b);
        }

        public static async Task DownloadTo(this RestRequest self, FileEntry targetFile) {
            using (var stream = await self.GetResult<Stream>()) {
                float totalBytes = (await self.GetResultHeaders()).GetFileSizeInBytesOnServer();
                var progressInPercent = new ChangeTracker<float>(0);
                await targetFile.SaveStreamAsync(stream, (savedBytes) => {
                    if (progressInPercent.SetNewValue(100 * savedBytes / totalBytes)) {
                        self.onProgress?.Invoke(progressInPercent.value);
                    }
                });
            }
        }

        public static RestRequest AddFileViaForm(this RestRequest self, FileEntry fileToUpload, string key = "file") {
            var fileStream = fileToUpload.OpenForRead();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") {
                Name = key,
                FileName = fileToUpload.Name
            };
            self.WithFormContent(new Dictionary<string, object>() { { key, streamContent } });
            return self;
        }

        public static RestRequest WithRequestHeaders(this RestRequest self, params KeyValuePair<string, string>[] headers) {
            var headersToAdd = new Dictionary<string, string>();
            foreach (var header in headers) { headersToAdd.Add(header.Key, header.Value); }
            self.WithRequestHeaders(new Headers(headersToAdd));
            return self;
        }

        public static RestRequest WithRequestHeader(this RestRequest self, string headerName, string headerValue) {
            return self.WithRequestHeaders(new KeyValuePair<string, string>(headerName, headerValue));
        }

        public static RestRequest WithRequestHeaderUserAgent(this RestRequest self, string userAgent) {
            return self.WithRequestHeader("user-agent", userAgent);
        }

        public static bool IsErrorStatus(this HttpStatusCode statusCode) {
            if (statusCode.IsClientError()) { return true; }
            if (statusCode.IsServerError()) { return true; }
            return false;
        }

        public static bool IsClientError(this HttpStatusCode statusCode) { return 400 <= (int)statusCode && (int)statusCode < 500; }
        public static bool IsServerError(this HttpStatusCode statusCode) { return 500 <= (int)statusCode; }

        /// <summary> Does its best to convert HTML to text, see https://stackoverflow.com/a/16407272/165106 </summary>
        public static string HtmlToPlainText(string html) {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);
            var text = html;
            //Decode html specific characters
            text = WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);
            return text;
        }

    }

    public class NoSuccessError : Exception {
        public HttpStatusCode statusCode;
        public NoSuccessError(HttpStatusCode statusCode, string message) : base(message) { this.statusCode = statusCode; }
        public override string ToString() { return statusCode + " - " + Message; }

        // Required default constructors (https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1032):
        public NoSuccessError() { }
        public NoSuccessError(string message) : base(message) { }
        public NoSuccessError(string message, Exception innerException) : base(message, innerException) { }

    }

}