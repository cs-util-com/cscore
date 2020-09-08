using com.csutil.datastructures;
using com.csutil.http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

    }

}
