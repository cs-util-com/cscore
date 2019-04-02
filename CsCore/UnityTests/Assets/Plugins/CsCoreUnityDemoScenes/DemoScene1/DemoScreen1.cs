using com.csutil;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DemoScreen1 : MonoBehaviour {

    private class MyClass1 {
        public string theCurrentTime;
        public int myInt;
    }

    void Start() {
        var links = gameObject.GetLinkMap();
        links.Get<Button>("ButtonTestJsonLib").SetOnClickAction(delegate {

            var key = "testObj1";
            var myObj = new MyClass1() { theCurrentTime = "It is " + DateTime.Now, myInt = 123 };

            PlayerPrefsV2.SetObject(key, myObj);
            AssertV2.AreEqual(myObj.theCurrentTime, PlayerPrefsV2.GetObject<MyClass1>(key, null).theCurrentTime);
            AssertV2.AreEqual(myObj.myInt, PlayerPrefsV2.GetObject<MyClass1>(key, null).myInt);

            links.Get<Text>("JsonOutput").text = JsonWriter.GetWriter().Write(PlayerPrefsV2.GetObject<MyClass1>(key, null));

        });
    }

}
