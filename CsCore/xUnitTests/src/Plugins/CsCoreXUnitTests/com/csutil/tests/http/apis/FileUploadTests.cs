using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http;
using Newtonsoft.Json;
using Xunit;
using Zio;

namespace com.csutil.tests.http {

    public class FileUploadTests {

        public FileUploadTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task TestPostFormData() {

            // Use postman-echo.com to test posting form data including files:
            RestRequest request = new Uri("https://postman-echo.com/post").SendPOST();

            DirectoryEntry dir = EnvironmentV2.instance.GetNewInMemorySystem();
            FileEntry fileToUpload = dir.GetChild("test.txt");
            fileToUpload.SaveAsText("I am a text");
            request.AddFileViaForm(fileToUpload);

            string formText = "I am a string";
            request.WithFormContent(new Dictionary<string, object>() { { "formString1", formText } });

            PostmanEchoResponse result = await request.GetResult<PostmanEchoResponse>();
            Log.d(JsonWriter.AsPrettyString(result));
            Assert.Single(result.form);
            Assert.Equal(formText, result.form.First().Value);
            Assert.Single(result.files);
            Assert.Equal(fileToUpload.Name, result.files.First().Key);
            Assert.True(result.headers.contentType.StartsWith("multipart/form-data"));

        }

        [Fact]
        public async Task TestPostFormData2() {

            // Use postman-echo.com to test posting form data including files:
            RestRequest request = new Uri("https://postman-echo.com/post").SendPOST();

            string formText1 = "I am a string 1";
            string formText2 = "I am a string 2";
            var formData = new Dictionary<string, object>();
            formData.Add("formString1", formText1);
            formData.Add("formString2", formText2);
            request.WithFormContent(formData);

            PostmanEchoResponse result = await request.GetResult<PostmanEchoResponse>();
            Log.d(JsonWriter.AsPrettyString(result));
            Assert.Equal(2, result.form.Count);
            Assert.Equal(formText1, result.form.First().Value);
            Assert.Equal(formText2, result.form.Skip(1).First().Value);
            Assert.Equal("application/x-www-form-urlencoded", result.headers.contentType);

        }

        [Fact]
        public async Task TestFileIoUpload() {

            DirectoryEntry dir = EnvironmentV2.instance.GetNewInMemorySystem();
            FileEntry fileToUpload = dir.GetChild("test.txt");
            string textInFile = "I am a text";
            fileToUpload.SaveAsText(textInFile);
            Assert.Equal(textInFile, fileToUpload.LoadAs<string>());

            RestRequest uploadRequest = new Uri("https://file.io/?expires=1d").SendPOST().AddFileViaForm(fileToUpload);
            uploadRequest.WithRequestHeaderUserAgent("" + GuidV2.NewGuid());
            FileIoResponse result = await uploadRequest.GetResult<FileIoResponse>();

            Log.d(JsonWriter.AsPrettyString(result));
            Assert.True(result.success);
            Assert.NotEmpty(result.link);

            FileEntry fileToDownloadTo = dir.GetChild("test2.txt");
            var downloadRequest = new Uri(result.link).SendGET();
            downloadRequest.WithRequestHeaderUserAgent("" + GuidV2.NewGuid());
            await downloadRequest.DownloadTo(fileToDownloadTo);
            //Assert.True(textInFile == fileToDownloadTo.LoadAs<string>(), "Invalid textInFile from " + result.link);
            Assert.Equal(textInFile, fileToDownloadTo.LoadAs<string>());

        }

#pragma warning disable 0649 // Variable is never assigned to, and will always have its default value
        private class FileIoResponse {
            public bool success;
            public string key;
            public string link;
            public string expiry;
            public int? error;
            public string message;
        }

        public class PostmanEchoResponse {

            public object args { get; set; }
            public object data { get; set; }
            public Dictionary<string, string> files { get; set; }
            public Dictionary<string, object> form { get; set; }
            public Headers headers { get; set; }
            public object json { get; set; }
            public Uri url { get; set; }

            public class Headers {
                public string host { get; set; }
                [JsonProperty("x-forwarded-proto")] public string xForwardedProto { get; set; }
                [JsonProperty("x-forwarded-port")] public string xForwardedPort { get; set; }
                [JsonProperty("x-amzn-trace-id")] public string xAmznTraceId { get; set; }
                [JsonProperty("content-length")] public string contentLength { get; set; }
                [JsonProperty("content-type")] public string contentType { get; set; }
            }

        }
#pragma warning restore 0649 // Variable is never assigned to, and will always have its default value

    }

}