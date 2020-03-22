using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
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
            Assert.NotEmpty(await tracker.store.GetAllKeys());

            var s = StopwatchV2.StartNewV2();
            for (int i = 0; i < 20; i++) {
                await TaskV2.Delay(500);
                if ((await tracker.store.GetAllKeys()).Count() == 0) { break; }
            }
            Assert.True(s.ElapsedMilliseconds < 5000, "s.ElapsedMilliseconds=" + s.ElapsedMilliseconds);

            Assert.Empty(await tracker.store.GetAllKeys());
            // Make sure the DefaultAppFlowImpl itself does not create more events while sending the existing ones: 
            for (int i = 0; i < 10; i++) {
                await TaskV2.Delay(100);
                var c1 = tracker.eventsThatWereSent.Count;
                await TaskV2.Delay(100);
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