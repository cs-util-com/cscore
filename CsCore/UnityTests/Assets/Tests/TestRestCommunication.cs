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

namespace com.csutil.http.tests {

    public class TestRestCommunication {

        [UnityTest]
        public IEnumerator TestResultCallback() {
            var www = UnityWebRequest.Get("https://httpbin.org/get");

            HttpBinGetResp a = null;
            yield return www.SendWebRequestV2(new Response<HttpBinGetResp>().WithResultCallback((x) => {
                Log.d("Your IP is " + x.origin);
                a = x;
            }));
            HttpBinGetResp b = www.GetResult<HttpBinGetResp>();

            var w = JsonWriter.NewWriter();
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
        public IEnumerator TestGetResultMethod() {
            var www = UnityWebRequest.Get("https://httpbin.org/get");

            var runningTask = www.SendV2().GetResult<HttpBinGetResp>(x => {
                Log.d("Your IP is " + x.origin);
            });
            while (!runningTask.IsCompleted) {
                Log.d("Waiting..");
                yield return new WaitForSeconds(0.1f);
            }
            var x2 = runningTask.Result;
            Log.d("Your IP is " + x2.origin);
        }

        public class HttpBinGetResp {
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
