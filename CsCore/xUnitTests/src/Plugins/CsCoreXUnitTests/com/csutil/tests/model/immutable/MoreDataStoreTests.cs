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

    public class StoreMiddlewareExecutionOrderTests {

        public StoreMiddlewareExecutionOrderTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task TestDispatchInListenerCallback() {

            var store = new DataStore<MyModel>(ReduceMyModel, new MyModel(0), NewMiddleware());

            store.Dispatch(new MyAction() { val = 1 });
            Assert.Throws<InvalidOperationException>(() => { store.Dispatch(new MyAction() { val = 0 }); });
            store.Dispatch(new MyAction() { val = 2 });

            store.AddStateChangeListener(x => x.val, (newVal) => {
                if (newVal == 3) {
                    store.Dispatch(new MyAction() { val = 4 });
                }
            }, false);
            store.Dispatch(new MyAction() { val = 3 });
            Assert.Equal(4, store.GetState().val);
        }

        private class MyModel {
            public readonly int val;
            public MyModel(int val) { this.val = val; }
        }

        private class MyAction {
            public int val;
        }

        private MyModel ReduceMyModel(MyModel previousState, object action) {
            if (action is MyAction a) {
                if (previousState.val + 1 != a.val) {
                    throw new InvalidOperationException($"{previousState.val} vs {a.val}");
                }
                return new MyModel(a.val);
            }
            return previousState;
        }

        /// <summary> Remember always the latest known val from the store and ensure it always grows by 1.
        /// This ensures that the middleware is first completed for action 1 before it starts for action 2 </summary>
        private Middleware<MyModel> NewMiddleware() {
            int val = 0;
            return (store) => {
                return (innerDispatcher) => {
                    return (action) => {
                        innerDispatcher(action);
                        if (val + 1 != store.GetState().val) {
                            throw new InvalidOperationException($"{val} vs {store.GetState().val}");
                        }
                        val = store.GetState().val;
                        return action;
                    };
                };
            };
        }

    }

}