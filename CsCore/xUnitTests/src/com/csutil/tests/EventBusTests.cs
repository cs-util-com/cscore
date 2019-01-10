using System;
using Xunit;

namespace com.csutil.tests {
    public class EventBusTests {

        [Fact]
        public void ExampleUsage1() {
            EventBus EventBus_instance = GetEventBusForTesting();

            var eventName = "TestEvent1";
            var val1ToSend = "Test 123";
            var val2ToSend = "Test 567";
            {
                var subscriber1 = new object();
                EventBus_instance.Subscribe<string>(subscriber1, eventName, (receivedVal) => {
                    Assert.Equal(val1ToSend, receivedVal);
                });
                var r = EventBus_instance.Publish(eventName, val1ToSend);
                Assert.Single(r);
            }
            {
                var subscriber2 = new object();
                EventBus_instance.Subscribe<string, string>(subscriber2, eventName, (val1, val2) => {
                    Assert.Equal(val1ToSend, val1);
                    Assert.Equal(val2ToSend, val2);
                });
                var r = EventBus_instance.Publish(eventName, val1ToSend, val2ToSend);
                Assert.Equal(2, r.Count);

                Assert.False(EventBus_instance.Unsubscribe(subscriber2, "eventThatDoesNotExist"));
                Assert.True(EventBus_instance.Unsubscribe(subscriber2, eventName));
                Assert.False(EventBus_instance.Unsubscribe(subscriber2, eventName));
            }
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

        /// <summary> 
        /// The global static EventBus.instance should not be used since the test
        /// System will execute many tests in parallel and other tests might change
        /// this global event bus randomly
        /// </summary>
        private static EventBus GetEventBusForTesting() {
            return new EventBus();
        }
    }
}
