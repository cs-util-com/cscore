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
            AppFlow.AddAppFlowTracker(new MyAppFlowTracker1());
            Log.MethodEntered(); // This will internally notify the AppFlow instance
            Assert.True((AppFlow.instance(null) as MyAppFlowTracker1).wasCalledByTestAppFlowTrackingTest);
        }

        private class MyAppFlowTracker1 : IAppFlow {
            public bool wasCalledByTestAppFlowTrackingTest = false;
            public void TrackEvent(string category, string action, object[] args) {
                if ("TestAppFlowTracking" == action) { wasCalledByTestAppFlowTrackingTest = true; }
            }
        }

        [Fact]
        public async Task TestDefaultAppFlowImplementation() {
            var tracker = new MyAppFlowTracker2(new InMemoryKeyValueStore().GetTypeAdapter<AppFlowEvent>());
            AppFlow.AddAppFlowTracker(tracker);
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

        [Fact]
        public async Task TestAppFlowToStore() {
            int eventCount = 1000;

            var s1 = Log.MethodEntered("InMemoryKeyValueStore");
            {
                IKeyValueStore store = new InMemoryKeyValueStore();
                AppFlowToStore appFlow = new AppFlowToStore(store.GetTypeAdapter<AppFlowEvent>());
                await TestAppFlowWithStore(eventCount, appFlow); // Run the tests
                Log.MethodDone(s1, maxAllowedTimeInMs: 200);
            }
            var s2 = Log.MethodEntered("InMemory FileSystem");
            {
                var dir1 = EnvironmentV2.instance.GetNewInMemorySystem().GetChildDir("TestAppFlowToFiles");
                IKeyValueStore store = new FileBasedKeyValueStore(dir1.CreateV2());
                AppFlowToStore appFlow = new AppFlowToStore(store.GetTypeAdapter<AppFlowEvent>());
                await TestAppFlowWithStore(eventCount, appFlow); // Run the tests
                Assert.Equal(eventCount, dir1.GetFiles().Count());
                Log.MethodDone(s2, maxAllowedTimeInMs: 4000);
            }
            var s3 = Log.MethodEntered("Real FileSystem");
            {
                var dir2 = EnvironmentV2.instance.GetRootTempFolder().GetChildDir("TestAppFlowToFiles");
                dir2.DeleteV2(); // Cleanup from last test
                IKeyValueStore store = new FileBasedKeyValueStore(dir2.CreateV2());
                AppFlowToStore appFlow = new AppFlowToStore(store.GetTypeAdapter<AppFlowEvent>());
                await TestAppFlowWithStore(eventCount, appFlow); // Run the tests
                Assert.Equal(eventCount, dir2.GetFiles().Count());
                Log.MethodDone(s3, maxAllowedTimeInMs: 5000);
            }
            Assert.True(s2.ElapsedMilliseconds < s3.ElapsedMilliseconds);
        }

        [Fact]
        public async Task TestLocalAnalytics() {
            int eventCount = 10000;

            // Create a LocalAnalytics instance that uses only memory stores for testing:
            LocalAnalytics localAnalytics = new LocalAnalytics(new InMemoryKeyValueStore());
            localAnalytics.createStoreFor = (_) => new InMemoryKeyValueStore().GetTypeAdapter<AppFlowEvent>();

            // Pass this local analytics system to the app flow impl. as the target store:
            AppFlowToStore appFlow = new AppFlowToStore(localAnalytics);
            await TestAppFlowWithStore(eventCount, appFlow); // Run the tests

            // Get the store that contains only the events of a specific category:
            var catMethodStore = localAnalytics.GetStoreForCategory(EventConsts.catMethod);
            { // Check that all events so far are of the method category:
                var all = await localAnalytics.GetAll();
                var allForCat = await catMethodStore.GetAll();
                Assert.Equal(all.Count(), allForCat.Count());
            }
            { // Add an event of a different category and check that the numbers again:
                appFlow.TrackEvent(EventConsts.catUi, "Some UI event");
                var all = await localAnalytics.GetAll();
                var allForCat = await catMethodStore.GetAll();
                Assert.Equal(all.Count(), allForCat.Count() + 1);
                var catUiStore = localAnalytics.GetStoreForCategory(EventConsts.catUi);
                Assert.Single(await catUiStore.GetAll());
            }

        }

        private async Task TestAppFlowWithStore(int eventCount, AppFlowToStore appFlow) {
            Assert.Empty(await appFlow.store.GetAllKeys());

            var t1 = Log.MethodEntered($"Track {eventCount} events via appFlow.TrackEvent");
            for (int i = 0; i < eventCount; i++) {
                appFlow.TrackEvent(EventConsts.catMethod, "action " + i);
            }
            Log.MethodDone(t1);

            var t2 = Log.MethodEntered("GetAll events in store");
            var events = await appFlow.store.GetAll();
            Assert.Equal(eventCount, events.Count());
            Log.MethodDone(t2);

            var t3 = Log.MethodEntered("GetAll again but for specific category");
            var filtered = events.Filter(x => x.cat == EventConsts.catMethod);
            Assert.Equal(filtered, events);
            Assert.Empty(events.Filter(x => x.cat == EventConsts.catUi));
            Log.MethodDone(t3);
        }

        private class MyAppFlowTracker2 : DefaultAppFlowImpl {

            public List<AppFlowEvent> eventsThatWereSent = new List<AppFlowEvent>();

            public MyAppFlowTracker2(KeyValueStoreTypeAdapter<AppFlowEvent> store) : base(store) { }

            protected override async Task<bool> SendEventToExternalSystem(AppFlowEvent appFlowEvent) {
                Log.d("SendEventToExternalSystem called with appFlowEvent=" + JsonWriter.AsPrettyString(appFlowEvent));
                await TaskV2.Delay(10); // Simulate that sending the event to an analytics server takes time
                eventsThatWereSent.Add(appFlowEvent);
                return true;
            }

        }

    }

}