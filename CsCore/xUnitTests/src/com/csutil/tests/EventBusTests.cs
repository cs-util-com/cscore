using System;
using Xunit;

namespace com.csutil.tests {

    public class EventBusTests {

        [Fact]
        public void ExampleUsage1() {

            // The EventBus can be accessed via EventBus.instance
            EventBus eventBus = GetEventBusForTesting();
            string eventName = "TestEvent1";

            //Register a subscriber for the eventName that gets notified when ever an event is send:
            object subscriber1 = new object(); // can be of any type
            eventBus.Subscribe(subscriber1, eventName, () => {
                Log.d("The event was received!");
            });

            // Now send out an event:
            eventBus.Publish(eventName);

            // When subscribers dont want to receive events anymore they can unsubscribe:
            eventBus.Unsubscribe(subscriber1, eventName);

        }

        [Fact]
        public void TestMultipleSubscribers() {

            EventBus EventBus_instance = GetEventBusForTesting();

            var myEventName = "TestEvent1";
            var myString1 = "Test 123";
            var myString2 = "Test 567";

            //Register a subscriber for the eventName that gets notified when ever an event is send:
            var subscriber1 = new object();
            EventBus_instance.Subscribe<string>(subscriber1, myEventName, (receivedString) => {
                Assert.Equal(myString1, receivedString);
            });

            // Now send out the event and pass myString1 as an event parameter:
            var publishResult1 = EventBus_instance.Publish(myEventName, myString1);
            Assert.Single(publishResult1);

            // Subscribe a second subscriber for the same eventName, which takes 2 arguments
            var subscriber2 = new object();
            EventBus_instance.Subscribe<string, string>(subscriber2, myEventName, (val1, val2) => {
                Assert.Equal(myString1, val1);
                Assert.Equal(myString2, val2);
            });

            // Publish another event with the same eventName:
            var publishResult2 = EventBus_instance.Publish(myEventName, myString1, myString2);
            // Both subscribers 1 and 2 reacted to the event so the publish results count will be 2:
            Assert.Equal(2, publishResult2.Count);

            // When the subscribers dont want to listen anymore to the event they can unsubscribe:
            Assert.True(EventBus_instance.Unsubscribe(subscriber1, myEventName));
            Assert.True(EventBus_instance.Unsubscribe(subscriber2, myEventName));

            // Unsubscribing to events that dont exist does not do anything:
            Assert.False(EventBus_instance.Unsubscribe(subscriber2, myEventName));
            Assert.False(EventBus_instance.Unsubscribe(subscriber2, "eventThatDoesNotExist"));

        }

        [Fact]
        public void TestGetSubscribersFor() {
            EventBus EventBus_instance = GetEventBusForTesting();
            var eventName = "TestEvent1";
            var val1ToSend = "Test 123";
            var val2ToSend = "Test 567";

            Assert.Empty(EventBus_instance.GetSubscribersFor(eventName));

            var subscriber = new object();
            EventBus_instance.Subscribe<string, string>(subscriber, eventName, (val1, val2) => {
                Assert.Equal(val1ToSend, val1);
                Assert.Equal(val2ToSend, val2);
            });
            var subs = EventBus_instance.GetSubscribersFor(eventName);
            Assert.True(subs.Contains(subscriber));

            var r = EventBus_instance.Publish(eventName, val1ToSend, val2ToSend);
            Assert.Single(r);
        }

        [Fact]
        public void TestSubscribeUnsubscribe() {
            EventBus EventBus_instance = GetEventBusForTesting();
            var eventName = "TestEvent1";
            var val1ToSend = "Test 123";

            var subscriber = new object();
            Assert.Empty(EventBus_instance.Publish(eventName, val1ToSend));
            EventBus_instance.Subscribe<string>(subscriber, eventName, (receivedVal) => {
                Assert.Equal(val1ToSend, receivedVal);
            });
            Assert.Single(EventBus_instance.Publish(eventName, val1ToSend));
            Assert.True(EventBus_instance.UnsubscribeAll(subscriber));
            Assert.False(EventBus_instance.UnsubscribeAll(subscriber));
        }

        [Fact]
        public void TestSubscribeForOnePublish() {
            EventBus EventBus_instance = GetEventBusForTesting();
            var eventName = "TestEvent1";
            // First an event will be published:
            Assert.Empty(EventBus_instance.Publish(eventName));
            var wasAlreadyReceived = false;
            // Afterwards a subscriber for the event will be added:
            EventBus_instance.SubscribeForOnePublish(new object(), eventName, () => {
                Assert.False(wasAlreadyReceived);
                wasAlreadyReceived = true;
            });
            // Now another event with the same eventName will be published:
            Assert.Single(EventBus_instance.Publish(eventName));
            // The subscriber registered for only a single event so the event will not have any subscribers anymore:
            Assert.Empty(EventBus_instance.Publish(eventName));
        }

        [Fact]
        public void TestSubscribeForOnePublishOrInstantInvokeIfInHistory() {
            EventBus EventBus_instance = GetEventBusForTesting();

            { // Test subscribing for a single event that was already published:
                var eventName1 = "TestEvent1";
                // Publish before subscribing:
                Assert.Empty(EventBus_instance.Publish(eventName1));
                var wasAlreadyReceived = false;
                // Now subscribe and instantly get the callback because the event was already published before:
                Assert.True(EventBus_instance.SubscribeForOnePublishOrInstantInvokeIfInHistory(eventName1, () => {
                    Assert.False(wasAlreadyReceived);
                    wasAlreadyReceived = true;
                }));
                // The subscriber now has already unsubscribed so a second publish will not have any subscribers:
                Assert.Empty(EventBus_instance.Publish(eventName1));
            }

            { // Test subscribing before the event was published:
                var eventName2 = "TestEvent2";
                var wasAlreadyReceived = false;
                Assert.False(EventBus_instance.SubscribeForOnePublishOrInstantInvokeIfInHistory(eventName2, () => {
                    Assert.False(wasAlreadyReceived);
                    wasAlreadyReceived = true;
                }));
                // The first time the subscriber will receive the event:
                Assert.Single(EventBus_instance.Publish(eventName2));
                // The second time the subscriber will already have unsubscribed:
                Assert.Empty(EventBus_instance.Publish(eventName2));
            }
        }

        [Fact]
        public void TestPerformance1() {

            // The EventBus can be accessed via EventBus.instance
            EventBus eventBus = GetEventBusForTesting();
            string eventName = "TestEvent1 - TestPerformance1";

            var receivedEventsCounter = 0;

            //Register a subscriber for the eventName that gets notified when ever an event is send:
            object subscriber1 = new object(); // can be of any type
            eventBus.Subscribe(subscriber1, eventName, () => {
                receivedEventsCounter++;
            });

            var timing = Log.MethodEntered();
            var nrOfEventsSendOut = 1000000;
            for (int i = 0; i < nrOfEventsSendOut; i++) {
                eventBus.Publish(eventName);
            }
            AssertV2.ThrowExeptionIfAssertionFails(() => {
                timing.AssertUnderXms(10000);
            });
            Assert.Equal(nrOfEventsSendOut, receivedEventsCounter);

        }

        /// <summary> 
        /// The global static EventBus.instance should not be used in tests since the test
        /// System will execute many tests in parallel and other tests might change
        /// this global event bus randomly
        /// </summary>
        private static EventBus GetEventBusForTesting() {
            return new EventBus();
        }
    }
}
