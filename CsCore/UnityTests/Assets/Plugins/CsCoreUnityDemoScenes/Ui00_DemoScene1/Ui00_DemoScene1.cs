using com.csutil.http;
using com.csutil.logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    public class Ui00_DemoScene1 : UnitTestMono {


        private Dictionary<string, Link> links;

        public override IEnumerator RunTest() {

            LogConsole.RegisterForAllLogEvents(this);

            links = gameObject.GetLinkMap();
            links.Get<Button>("ButtonTestJsonLib").SetOnClickAction(delegate { TestJsonSerialization(); });
            links.Get<Button>("ButtonTestPing").SetOnClickAction(delegate {
                StartCoroutine(TestCurrentPing(links.Get<InputField>("IpInput").text));
            });

            yield return new WaitForSeconds(0.5f);
            SimulateButtonClickOn("ButtonTestJsonLib");

            yield return new WaitForSeconds(0.5f);
            SimulateButtonClickOn("ButtonTestPing");

        }

        private class MyClass1 {
            public string theCurrentTime;
            public int myInt;
        }

        private void TestJsonSerialization() {
            var t = Log.MethodEntered();
            var prefsKey = "testObj1";
            var myObj = new MyClass1() { theCurrentTime = "It is " + DateTimeV2.Now, myInt = 123 };
            PlayerPrefsV2.SetObject(prefsKey, myObj);
            AssertV2.AreEqual(myObj.theCurrentTime, PlayerPrefsV2.GetObject<MyClass1>(prefsKey, null).theCurrentTime);
            AssertV2.AreEqual(myObj.myInt, PlayerPrefsV2.GetObject<MyClass1>(prefsKey, null).myInt);
            links.Get<Text>("JsonOutput").text = JsonWriter.GetWriter().Write(PlayerPrefsV2.GetObject<MyClass1>(prefsKey, null));
            Log.MethodDone(t);
        }

        private IEnumerator TestCurrentPing(string ipOrUrl) {
            Log.d("Will ping now ipOrUrl=" + ipOrUrl);
            var pingTask = RestFactory.instance.GetCurrentPing(ipOrUrl);
            yield return pingTask.AsCoroutine();
            links.Get<Text>("PingOutput").text = "Current Ping: " + pingTask.Result + "ms";
        }

    }

}