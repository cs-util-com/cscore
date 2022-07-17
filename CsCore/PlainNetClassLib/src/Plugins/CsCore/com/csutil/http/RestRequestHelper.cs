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
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Zio;

namespace com.csutil {

    public static class RestRequestHelper {

        public static RestRequest WithJsonContent(this RestRequest self, object jsonContent) {
            return self.WithJsonContent(JsonWriter.GetWriter(jsonContent).Write(jsonContent));
        }

        public static RestRequest WithJsonContent(this RestRequest self, string jsonContent) {
            return self.WithTextContent(jsonContent, Encoding.UTF8, "application/json");
        }

        public static Uri WithAddedQueryParams(this Uri self, Dictionary<string, object> queryParams) {
            var uriBuilder = new UriBuilder(self);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var param in queryParams) { query[param.Key] = "" + param.Value; }
            uriBuilder.Query = query.ToString();
            return uriBuilder.Uri;
        }

        public static string ToUriEncodedString(object o) {
            var map = JsonReader.GetReader().Read<Dictionary<string, object>>(JsonWriter.GetWriter(o).Write(o));
            return map.Select((x) => x.Key + "=" + Uri.EscapeDataString("" + x.Value)).Aggregate((a, b) => a + "&" + b);
        }

        public static async Task DownloadTo(this RestRequest self, FileEntry targetFile) {
            using (var stream = await self.GetResult<Stream>()) {
                float totalBytes = (await self.GetResultHeaders()).GetFileSizeInBytesOnServer();
                AssertV2.IsTrue(totalBytes > 0, "GetFileSizeInBytesOnServer totalBytes=" + totalBytes);
                var progressInPercent = new ChangeTracker<float>(0f);
                await targetFile.SaveStreamAsync(stream, (savedBytes) => {
                    float percentValue = 100f * savedBytes / totalBytes;
                    AssertV2.IsInRange(0, percentValue, 100, "percentValue");
                    percentValue = Math.Max(0f, percentValue);
                    if (progressInPercent.SetNewValue(percentValue)) {
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
            self.RequestStartedTask.ContinueWith(delegate {
                if (self.onProgress != null) {
                    fileStream.MonitorPositionForProgress(progress => {
                        self.onProgress(progress);
                    }, self.CancellationTokenSource).LogOnError();
                }
            });
            return self;
        }

        public static RestRequest WithRequestHeaders(this RestRequest self, params KeyValuePair<string, string>[] requestHeaders) {
            var headersToAdd = new Dictionary<string, string>();
            foreach (var header in requestHeaders) { headersToAdd.Add(header.Key, header.Value); }
            return WithRequestHeaders(self, headersToAdd);
        }
        
        public static RestRequest WithRequestHeaders(RestRequest self, Dictionary<string, string> requestHeaders) {
            self.WithRequestHeaders(new Headers(requestHeaders));
            return self;
        }

        public static RestRequest WithRequestHeader(this RestRequest self, string headerName, string headerValue) {
            return self.WithRequestHeaders(new KeyValuePair<string, string>(headerName, headerValue));
        }

        public static RestRequest WithRequestHeaderUserAgent(this RestRequest self, string userAgent) {
            return self.WithRequestHeader("user-agent", userAgent);
        }

        /// <summary> See e.g. https://jwt.io/introduction/ </summary>
        public static RestRequest WithRequestHeaderJwt(this RestRequest self, string jwt) {
            return self.WithRequestHeader("Authorization", "Bearer " + jwt);
        }
        
        public static RestRequest WithAuthorization(this RestRequest self, string key) {
            return self.WithRequestHeader("Authorization", "Bearer " + key);
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