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
                EventBus_instance.Subscribe<string>(new object(), eventName, (receivedVal) => {
                    Assert.Equal(val1ToSend, receivedVal);
                });
                var r = EventBus_instance.Publish(eventName, val1ToSend);
                Assert.Single(r);
            }
            {
                EventBus_instance.Subscribe<string, string>(new object(), eventName, (val1, val2) => {
                    Assert.Equal(val1ToSend, val1);
                    Assert.Equal(val2ToSend, val2);
                });
                var r = EventBus_instance.Publish(eventName, val1ToSend, val2ToSend);
                Assert.Equal(2, r.Count);
            }
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
