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
    public class RestCommunicationTests {

        [UnityTest]
        public IEnumerator Test1() {
            var normalUnityWebRequest = UnityWebRequest.Get("https://httpbin.org/get");

            var runningTask = normalUnityWebRequest.SendV2().GetResult<HttpBinGetResp>(x => {
                Log.d("Your IP is " + x.origin);
            });
            while (!runningTask.IsCompleted) {
                Log.d("Waiting..");
                yield return new WaitForSeconds(0.1f);
            }
            var x2 = runningTask.Result;
            Log.d("Your IP is " + x2.origin);
            yield return null;
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
