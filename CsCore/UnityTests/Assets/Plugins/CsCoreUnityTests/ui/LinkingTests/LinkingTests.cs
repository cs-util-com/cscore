using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class LinkingTests {

        [Test]
        public void ExampleUsage1() {

            // Load a prefab that contains Link MonoBehaviours:
            GameObject prefab = ResourcesV2.LoadPrefab("ExamplePrefab1.prefab");

            // Collect all Link MonoBehaviours in the prefab:
            Dictionary<string, Link> links = prefab.GetLinkMap();

            // In the Prefab Link-Monos are placed in all GameObjects that need 
            // to be accessed by the code. Links have a id to reference them:
            // Via the Link.id the objects can quickly be accessed: 
            Assert.IsNotNull(links.Get<GameObject>("Button 1"));

            // The GameObject "Button 1" contains a Button-Mono that can be accessed:
            Button button1 = links.Get<Button>("Button 1");
            button1.SetOnClickAction(delegate {
                Log.d("Button 1 clicked");
            });

            // The prefab also contains other Links in other places to quickly setup the UI:
            links.Get<Text>("Text 1").text = "Some text";
            links.Get<Toggle>("Toggle 1").SetOnValueChangedAction((isNowChecked) => {
                Log.d("Toggle 1 is now " + (isNowChecked ? "checked" : "unchecked"));
                return true;
            });

        }

        [Test]
        public void TestOnValueChangedListeners() {
            GameObject prefab = ResourcesV2.LoadPrefab("ExamplePrefab1.prefab");
            Dictionary<string, Link> links = prefab.GetLinkMap();
            {
                var toggle = links.Get<Toggle>("Toggle 1");
                toggle.isOn = false;
                var counter = 0;
                toggle.SetOnValueChangedAction((isNowChecked) => {
                    counter++;
                    if (isNowChecked) { return true; }
                    return false;
                });
                Assert.AreNotEqual(true, toggle.isOn);
                toggle.isOn = true;
                Assert.AreEqual(true, toggle.isOn);
                Assert.AreEqual(1, counter);
                toggle.isOn = false;
                Assert.AreEqual(true, toggle.isOn);
                Assert.AreEqual(2, counter);
            }
            {
                var input = links.Get<InputField>("Input Field 1");
                input.text = "";
                var counter = 0;
                input.SetOnValueChangedAction((newValue) => {
                    counter++;
                    if (newValue == "1") { return true; }
                    Assert.AreEqual("2", newValue);
                    return false;
                });
                Assert.AreNotEqual("1", input.text);
                input.text = "1";
                Assert.AreEqual("1", input.text);
                Assert.AreEqual(1, counter);
                input.text = "2";
                Assert.AreEqual("1", input.text);
                Assert.AreEqual(2, counter);
            }
        }

        [Test]
        public void TestLoadingPrefabs() {
            // Load the ExamplePrefab1.prefab located in Assets\Tests\TestLinking\Resources :
            Assert.IsNotNull(ResourcesV2.LoadPrefab("ExamplePrefab1"));
            // Loading a prefab that does not exist results in an error:
            AssertV2.Throws<Exception>(() => { ResourcesV2.LoadPrefab("ExamplePrefab2"); });
        }

        [Test]
        public void TestLinkMaps() {
            bool prefabLoadedEventReceived = false;
            EventBus.instance.Subscribe(new object(), IoEvents.PREFAB_LOADED, () => { prefabLoadedEventReceived = true; });
            GameObject prefab = ResourcesV2.LoadPrefab("ExamplePrefab1.prefab");
            Assert.IsTrue(prefabLoadedEventReceived);

            bool linkMapCreationEventReceived = false;
            EventBus.instance.Subscribe(new object(), LinkingEvents.LINK_MAP_CREATED, () => { linkMapCreationEventReceived = true; });
            var links = prefab.GetLinkMap();
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
