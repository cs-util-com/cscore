using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests.eventbus {

    public class EventbusTests {

        [Test]
        public void ExampleUsage1() {

            // GameObjects can subscribe to events:
            var myGameObject = new GameObject("MyGameObject 1");
            myGameObject.Subscribe("MyEvent1", () => {
                Log.d("I received the event because I'm active");
            });

            // Behaviours can subscribe to events too:
            var myExampleMono = myGameObject.GetOrAddComponent<MyExampleMono1>();
            myExampleMono.Subscribe("MyEvent1", () => {
                Log.d("I received the event because I'm enabled and active");
            });

            // The broadcast will reach both the GameObject and the MonoBehaviour:
            EventBus.instance.Publish("MyEvent1");

        }

        [Test]
        public void SubscribingWithGameObjects() {
            var myGameObject = new GameObject();
            var event1 = "MyEvent1 - SubscribingWithGameObjects";
            var counter = 0;

            myGameObject.Subscribe(event1, () => {
                Log.d("Event1 was received!");
                counter++;
            });

            Assert.AreEqual(0, counter);
            EventBus.instance.Publish(event1);
            Assert.AreEqual(1, counter);

            myGameObject.SetActive(false);
            EventBus.instance.Publish(event1); // event should not be received
            Assert.AreEqual(1, counter); // because gameObject is inactive

            myGameObject.SetActive(true);
            EventBus.instance.Publish(event1, "I am an ignored parameter");
            Assert.AreEqual(2, counter);

            myGameObject.Destroy();
            EventBus.instance.Publish(event1); // event should not be received
            Assert.AreEqual(2, counter); // because gameObject is destroyed
        }

        [Test]
        public void SubscribingWithMonoBehaviour() {
            var someMono = new GameObject().GetOrAddComponent<MyExampleMono1>();

            var event1 = "MyEvent1 - SubscribingWithMonoBehaviour";
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

        [UnityTest]
        public IEnumerator TestSubscribeOnMainThread() {
            string eventName = "EventSendFromBackgroundThread";
            var counter = 0;
            EventBus.instance.Subscribe(new object(), eventName, () => {
                Assert.IsFalse(MainThread.isMainThread);
                counter++;
            });
            EventBus.instance.SubscribeOnMainThread(new object(), eventName, () => {
                Assert.IsTrue(MainThread.isMainThread);
                counter++;
            });
            yield return TaskRunner.instance.RunInBackground(async (cancel) => {
                EventBus.instance.Publish(eventName);
                Assert.AreEqual(1, counter);
                await Task.CompletedTask;
            }).AsCoroutine();
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(2, counter);
        }

    }
}
