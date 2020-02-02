using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.logging;
using com.csutil.logging.analytics;
using Xunit;

namespace com.csutil.tests {

    public class AppFlowTests {

        public AppFlowTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void TestAppFlowTracking() {
            AppFlow.instance = new MyAppFlowTracker1();
            Log.MethodEntered(); // This will internally notify the AppFlow instance
            Assert.True((AppFlow.instance as MyAppFlowTracker1).wasCalledByTestAppFlowTrackingTest);
        }

        private class MyAppFlowTracker1 : IAppFlow {
            public bool wasCalledByTestAppFlowTrackingTest = false;
            public void TrackEvent(string category, string action, object[] args) {
                if ("TestAppFlowTracking" == action) { wasCalledByTestAppFlowTrackingTest = true; }
            }
        }

        [Fact]
        public async Task TestDefaultAppFlowImplementation() {
            var tracker = new MyAppFlowTracker2(new InMemoryKeyValueStore());
            AppFlow.instance = tracker;
            Log.MethodEntered(); // This will internally notify the AppFlow instance
            Assert.True((await tracker.store.GetAllKeys()).Count() > 0);
            await Task.Delay(3000);
            // After 3 seconds all events should have been sent to the external system:
            Assert.Empty(await tracker.store.GetAllKeys());
            // Make sure the DefaultAppFlowImpl itself does not create more events while sending the existing ones: 
            for (int i = 0; i < 5; i++) {
                await Task.Delay(100);
                var c1 = tracker.eventsThatWereSent.Count;
                await Task.Delay(100);
                var c2 = tracker.eventsThatWereSent.Count;
                Assert.Equal(c1, c2);
            }
        }

        private class MyAppFlowTracker2 : DefaultAppFlowImpl {

            public List<AppFlowEvent> eventsThatWereSent = new List<AppFlowEvent>();

            public MyAppFlowTracker2(IKeyValueStore store) : base(store) { }

            protected override async Task<bool> SendEventToExternalSystem(AppFlowEvent appFlowEvent) {
                Log.d("SendEventToExternalSystem called with appFlowEvent=" + JsonWriter.AsPrettyString(appFlowEvent));
                await TaskV2.Delay(10); // Simulate that sending the event to an analytics server takes time
                eventsThatWereSent.Add(appFlowEvent);
                return true;
            }
        }

    }
}
