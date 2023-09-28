using Xunit;

namespace com.csutil.integrationTests {
    
    public class EventBusPerformanceTests {
        
        [Fact]
        public void TestPerformance1() {

            // The EventBus can be accessed via EventBus.instance
            EventBus eventBus = new EventBus();
            string eventName = "TestEvent1 - TestPerformance1";

            var receivedEventsCounter = 0;

            //Register a subscriber for the eventName that gets notified when ever an event is send:
            object subscriber1 = new object(); // can be of any type
            eventBus.Subscribe(subscriber1, eventName, () => {
                receivedEventsCounter++;
            });

            var timing = Log.MethodEntered("EventBusTests.TestPerformance1");
            var nrOfEventsSendOut = 1000000;
            for (int i = 0; i < nrOfEventsSendOut; i++) {
                eventBus.Publish(eventName);
            }
            Assert.True(timing.IsUnderXms(10000), "timing in ms was: " + timing.ElapsedMilliseconds);
            Assert.Equal(nrOfEventsSendOut, receivedEventsCounter);

        }
        
    }
    
}