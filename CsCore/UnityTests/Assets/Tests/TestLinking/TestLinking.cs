using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
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
            Assert.IsNotNull(ResourcesV2.LoadPrefab("ExamplePrefab1"));
            AssertV2.Throws<Exception>(() => { ResourcesV2.LoadPrefab("ExamplePrefab2"); });
        }

        [Test]
        public void TestLinkMap() {
            bool prefabLoadedEventReceived = false;
            EventBus.instance.Subscribe(new object(), IoEvents.PREFAB_LOADED, () => { prefabLoadedEventReceived = true; });
            var p = ResourcesV2.LoadPrefab("ExamplePrefab1");
            Assert.IsTrue(prefabLoadedEventReceived);

            bool linkMapCreationEventReceived = false;
            EventBus.instance.Subscribe(new object(), LinkingEvents.LINK_MAP_CREATED, () => { linkMapCreationEventReceived = true; });
            var links = p.GetLinkMap();
            Assert.IsTrue(linkMapCreationEventReceived);

            Assert.IsNotNull(links.Get<Button>("Button 1"));
            Assert.IsNotNull(links.Get<GameObject>("Button 1"));
            AssertV2.Throws<Exception>(() => { links.Get<Button>("Button 2"); });

            links.Get<Text>("Text 1").text = "Some text";
            Assert.AreEqual("Some text", links.Get<Text>("Text 1").text);
            links.Get<Button>("Button 1").SetOnClickAction(delegate {
                Log.d("Button 1 clicked");
            });
            links.Get<Toggle>("Toggle 1").SetOnValueChangedAction((isNowChecked) => {
                Log.d("Toggle 1 is now " + (isNowChecked ? "checked" : "unchecked"));
                return true;
            });

        }

    }

}
