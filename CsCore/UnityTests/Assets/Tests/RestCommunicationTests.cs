using com.csutil.http;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace com.csutil.tests.http {

    public class RestCommunicationTests {

        [SetUp]
        public void BeforeEachTest() { }

        [TearDown]
        public void AfterEachTest() { }

        [UnityTest]
        public IEnumerator TestResultCallback() {
            var www = UnityWebRequest.Get("https://httpbin.org/get");

            HttpBinGetResp a = null;
            yield return www.SendWebRequestV2(new Response<HttpBinGetResp>().WithResultCallback((x) => {
                Log.d("Your IP is " + x.origin);
                a = x;
            }));
            HttpBinGetResp b = www.GetResult<HttpBinGetResp>();

            var w = JsonWriter.GetWriter();
            Assert.AreEqual(w.Write(a), w.Write(b));
        }

        [UnityTest]
        public IEnumerator TestProgress() {
            var requestProgressUpdateCounter = 0;
            var resp = new Response<HttpBinGetResp>().WithProgress((p) => {
                Log.d("Now progress=" + p + "%");
                requestProgressUpdateCounter++;
            });

            var www = UnityWebRequest.Get("https://httpbin.org/get");
            yield return www.SendWebRequestV2(resp);

            var result = resp.getResult();
            Assert.NotNull(result.origin);
            Assert.AreEqual(100, resp.progressInPercent.value);
            Assert.NotNull(requestProgressUpdateCounter > 0, "progressUpdateCounter=" + requestProgressUpdateCounter);
        }

        [UnityTest]
        public IEnumerator TestImageDownload() {
            var imgWidth = 1024;
            var imgHeight = 1024;

            var response = new Response<Texture>();
            var www = UnityWebRequestTexture.GetTexture("https://picsum.photos/" + imgWidth + "/" + imgHeight, true);

            yield return www.SendWebRequestV2(response);

            Assert.AreEqual(100, response.progressInPercent.value);
            Assert.IsTrue(response.duration.ElapsedMilliseconds > 100, "resp.duration=" + response.duration.ElapsedMilliseconds + "ms");

            var result = response.getResult();
            Assert.IsNotNull(result);
            Assert.AreEqual(imgWidth, result.width);
            Assert.AreEqual(imgHeight, result.height);
        }

        [UnityTest]
        public IEnumerator TestGetViaUnityWebRequest() {
            RestRequest request = UnityWebRequest.Get("https://httpbin.org/get").SendV2();
            yield return AssertGetResult(request);
        }

        [UnityTest]
        public IEnumerator TestGetViaUri() {
            Assert.AreEqual(typeof(UnityRestFactory), RestFactory.instance.GetType());
            RestRequest request = new Uri("https://httpbin.org/get").SendGET();
            yield return AssertGetResult(request);
        }

        private IEnumerator AssertGetResult(RestRequest get) {
            var runningTask = get.GetResult<HttpBinGetResp>(x => {
                Log.d("Your IP is " + x.origin);
            });
            while (!runningTask.IsCompleted) {
                Log.d("AssertGetResult: Waiting..");
                yield return new WaitForSeconds(0.1f);
            }
            Assert.IsTrue(runningTask.IsCompleted);
            Assert.IsFalse(runningTask.IsFaulted);
            Assert.IsFalse(runningTask.IsCanceled);

            var httpBinGetResponse = runningTask.Result;
            Log.d("Your IP is " + httpBinGetResponse.origin);
            Assert.IsNotNull(httpBinGetResponse.origin);
        }

        public class HttpBinGetResp {
            // The property names are based on the https://httpbin.org/get json response
            public Dictionary<string, object> args { get; set; }
            public string origin { get; set; }
            public string url { get; set; }
            public Headers headers { get; set; }
            public class Headers {
                public string Accept { get; set; }
                public string Accept_Encoding { get; set; }
                public string Accept_Language { get; set; }
                public string Connection { get; set; }
                public string Host { get; set; }
                public string Upgrade_Insecure_Requests { get; set; }
                public string User_Agent { get; set; }
            }
        }

    }

}
