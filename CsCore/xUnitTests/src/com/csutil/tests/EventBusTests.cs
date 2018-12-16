using System;
using Xunit;

namespace com.csutil.tests {
    public class EventBusTests : IDisposable {

        public EventBusTests() { // // Setup before each test:
            // Reset Eventbus:
            EventBus.instance = new EventBus();
        }

        public void Dispose() { // TearDown after each test:
        }

        [Fact]
        public void Test1() {
            var eventName = "TestEvent1";
            var val1ToSend = "Test 123";
            var val2ToSend = "Test 567";
            {
                EventBus.instance.Subscribe<string>(new object(), eventName, (receivedVal) => {
                    Assert.Equal(val1ToSend, receivedVal);
                });
                var r = EventBus.instance.Publish(eventName, val1ToSend);
                Assert.Single(r);
            }
            {
                EventBus.instance.Subscribe<string, string>(new object(), eventName, (val1, val2) => {
                    Assert.Equal(val1ToSend, val1);
                    Assert.Equal(val2ToSend, val2);
                });
                var r = EventBus.instance.Publish(eventName, val1ToSend, val2ToSend);
                Assert.Equal(2, r.Count);
            }
        }
    }
}
