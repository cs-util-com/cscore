using com.csutil;
using com.csutil.http;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoScreen1 : MonoBehaviour {

    private Dictionary<string, Link> links;

    void Start() {
        links = gameObject.GetLinkMap();
        links.Get<Button>("ButtonTestJsonLib").SetOnClickAction(delegate { TestJsonSerialization(); });
        links.Get<Button>("ButtonTestPing").SetOnClickAction(delegate {
            StartCoroutine(TestCurrentPing(links.Get<InputField>("IpInput").text));
        });
    }

    private class MyClass1 {
        public string theCurrentTime;
        public int myInt;
    }

    private void TestJsonSerialization() {
        var prefsKey = "testObj1";
        var myObj = new MyClass1() { theCurrentTime = "It is " + DateTime.Now, myInt = 123 };
        PlayerPrefsV2.SetObject(prefsKey, myObj);
        AssertV2.AreEqual(myObj.theCurrentTime, PlayerPrefsV2.GetObject<MyClass1>(prefsKey, null).theCurrentTime);
        AssertV2.AreEqual(myObj.myInt, PlayerPrefsV2.GetObject<MyClass1>(prefsKey, null).myInt);
        links.Get<Text>("JsonOutput").text = JsonWriter.GetWriter().Write(PlayerPrefsV2.GetObject<MyClass1>(prefsKey, null));
    }

    private IEnumerator TestCurrentPing(string ipOrUrl) {
        Log.d("Will ping now ipOrUrl=" + ipOrUrl);
        var pingTask = RestFactory.instance.GetCurrentPing(ipOrUrl);
        yield return pingTask.AsCoroutine();
        links.Get<Text>("PingOutput").text = "Current Ping: " + pingTask.Result + "ms";
    }

}
