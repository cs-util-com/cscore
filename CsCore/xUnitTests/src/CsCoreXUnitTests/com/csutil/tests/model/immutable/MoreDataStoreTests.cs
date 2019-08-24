using System;
using System.Threading.Tasks;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    public class MoreDataStoreTests {

        public MoreDataStoreTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task TestReplayRecorder() {

            // Add a recorder middleware to enable hot reload by replaying previously recorded actions:
            var recorder = new ReplayRecorder<string>();
            var recMiddleware = recorder.CreateMiddleware();
            // Test that exceptions are thrown if ReplayStore is not yet ready to use:
            await Assert.ThrowsAsync<NullReferenceException>(async () => { await recorder.ReplayStore(); });
            var store = new DataStore<string>(StateReducer, "", recMiddleware);
            await recorder.ReplayStore(); // Does nothing but does not throw an exception

            // Test that exceptions are thrown if an invalid nrOfActionsToReplay is passed into ReplayStore():
            await Assert.ThrowsAsync<ArgumentException>(async () => {
                await recorder.ReplayStore(delayBetweenStepsInMs: 0, nrOfActionsToReplay: 1);
            });

            // Test that Replay does not change the recorder.recordedActionsCount:
            Assert.Equal(0, recorder.recordedActionsCount);
            store.Dispatch("1"); // Dispatch a first event
            Assert.Equal(1, recorder.recordedActionsCount); // Now the count must be 1
            await recorder.ReplayStore();
            Assert.Equal(1, recorder.recordedActionsCount); // Recorded actions still 1 after replay

        }

        private string StateReducer(string previousState, object action) {
            if (action is ResetStoreAction) { return ""; }
            if (action is string s) { return s; }
            return previousState;
        }
    }

}