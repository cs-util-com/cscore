using com.csutil.http;
using com.csutil.keyvaluestore;
using com.csutil.logging;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    public class Ui00_DemoScene1 : UnitTestMono {

        private Dictionary<string, Link> links;

        public override IEnumerator RunTest() {

            IoC.inject.SetSingleton<IPreferences>(PlayerPrefsStore.NewPreferencesUsingPlayerPrefs());
            LogConsole.RegisterForAllLogEvents(this);

            links = gameObject.GetLinkMap();
            links.Get<Button>("ButtonTestJsonLib").SetOnClickAction(delegate {
                TestJsonSerialization().LogOnError();
            });
            links.Get<Button>("ButtonTestPing").SetOnClickAction(delegate {
                StartCoroutine(TestCurrentPing(links.Get<InputField>("IpInput").text));
            });
            links.Get<Button>("ButtonShowToast").SetOnClickAction(delegate {
                Toast.Show("Hello World");
            });
            // Clicking multiple times on a button with an async action will only execute the first click:
            links.Get<Button>("ButtonRunAsyncMethod").SetOnClickAction(async delegate {
                await Task.Delay(2000);
                Toast.Show("Button waited 2 seconds");
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

        private async Task TestJsonSerialization() {
            var t = Log.MethodEntered();
            var prefsKey = "testObj1";
            var myObj = new MyClass1() { theCurrentTime = "It is " + DateTimeV2.Now, myInt = 123 };
            await Preferences.instance.Set(prefsKey, myObj);
            AssertV2.AreEqual(myObj.theCurrentTime, (await Preferences.instance.Get<MyClass1>(prefsKey, null)).theCurrentTime);
            AssertV2.AreEqual(myObj.myInt, (await Preferences.instance.Get<MyClass1>(prefsKey, null)).myInt);
            links.Get<Text>("JsonOutput").text = JsonWriter.GetWriter().Write(await Preferences.instance.Get<MyClass1>(prefsKey, null));
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