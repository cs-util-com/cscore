using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace com.csutil.linking.tests {

    public class TestLinking {

        private class MySingleton { }

        [SetUp]
        public void BeforeEachTest() {
            UnitySetup.instance.Setup();
        }

        [TearDown]
        public void AfterEachTest() { }

        [Test]
        public void TestLoadingPrefab() {
            var p = ResourcesV2.LoadPrefab("ExamplePrefab1");
            var links = p.GetLinkMap();
            links.Get<Button>("Button 1").SetOnClickAction(delegate {
                Log.d("Button 1 clicked");
            });
            links.Get<Text>("Text 1").text = "Some text";
        }

    }

}
