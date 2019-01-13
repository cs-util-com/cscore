using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil.eventbus.tests {
    public class TestEventbus {

        private class MySingleton { }

        [SetUp]
        public void BeforeEachTest() { }

        [TearDown]
        public void AfterEachTest() { }

        [Test]
        public void SubscribingWithGameObjects() {
            var go = new GameObject();
            var event1 = "MyEvent1";
            var counter = 0;

            go.Subscribe(event1, () => {
                Log.d("Event1 was received!");
                counter++;
            });

            Assert.AreEqual(0, counter);
            EventBus.instance.Publish(event1);
            Assert.AreEqual(1, counter);

            go.SetActive(false);
            EventBus.instance.Publish(event1); // event should not be received
            Assert.AreEqual(1, counter); // because gameObject is inactive

            go.SetActive(true);
            EventBus.instance.Publish(event1, "I am an ignored parameter");
            Assert.AreEqual(2, counter);

            go.Destroy();
            EventBus.instance.Publish(event1); // event should not be received
            Assert.AreEqual(2, counter); // because gameObject is destroyed
        }

        [Test]
        public void SubscribingWithMonoBehaviour() {
            var someMono = new GameObject().GetOrAddComponent<TaskRunner>();

            var event1 = "MyEvent1";
            var counter = 0;

            someMono.Subscribe(event1, (string s) => {
                Log.d("Event1 was received with s='" + s + "'");
                counter++;
                return 99;
            });

            Assert.AreEqual(0, counter);
            var results = EventBus.instance.Publish(event1, "s1");
            Assert.AreEqual(1, counter);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(99, results.First());

            someMono.enabled = (false);
            EventBus.instance.Publish(event1, "s1"); // event should not be received
            Assert.AreEqual(1, counter); // because gameObject is inactive

            someMono.enabled = (true);
            EventBus.instance.Publish(event1, "s2", "I am an ignored parameter");
            Assert.AreEqual(2, counter);

            someMono.gameObject.Destroy();
            EventBus.instance.Publish(event1, "s2"); // event should not be received
            Assert.AreEqual(2, counter); // because gameObject is destroyed
        }

    }
}
