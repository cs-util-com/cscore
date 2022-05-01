using System;
using System.Collections.Generic;
using System.Threading;
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

    public class MultiThreadingStoreDispatchTests {

        public MultiThreadingStoreDispatchTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task TestDispatchFromManyThreads() {
            var store = new DataStore<MyModel>(ReduceMyModel, new MyModel(0), NewMiddleware());
            var oldVal = 0;
            store.AddStateChangeListener(x => x.val, (newVal) => {
                if (newVal != (oldVal + 1)) {
                    throw new InvalidOperationException($"{oldVal} vs {newVal}");
                }
                oldVal = newVal;
            }, false);
            {
                var count = 100;
                var tasks = new List<Task>();
                for (int i = 0; i < count; i++) {
                    tasks.Add(TaskV2.Run(() => {
                        store.Dispatch(new MyAction() { val = 1 });
                    }));
                }
                await Task.WhenAll(tasks);
                Assert.Equal(count, store.GetState().val);
            }
            {
                var tasks = new List<Task>();
                var tasks2 = new List<Task>();
                store.AddStateChangeListener(x => x.val, (newVal) => {
                    if (newVal <= 300) {
                        store.Dispatch(new MyAction() { val = 1 });
                        tasks2.Add(TaskV2.Run(() => {
                            store.Dispatch(new MyAction() { val = 1 });
                        }));
                    }
                }, false);
                var count = 100;
                for (int i = 0; i < count; i++) {
                    store.Dispatch(new MyAction() { val = 1 });
                    tasks.Add(TaskV2.Run(() => {
                        store.Dispatch(new MyAction() { val = 1 });
                    }));
                }
                await Task.WhenAll(tasks);
                await Task.WhenAll(tasks2);
                Assert.Equal(count * 7, store.GetState().val);
            }
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
                var prevVal = previousState.val;
                Thread.Sleep(5);
                return new MyModel(prevVal + a.val);
            }
            return previousState;
        }

        /// <summary> Remember always the latest known val from the store and ensure it always grows by 1.
        /// This ensures that the middleware is first completed for action 1 before it starts for action 2 </summary>
        private Middleware<MyModel> NewMiddleware() {
            return (store) => {
                return (innerDispatcher) => {
                    return (action) => {
                        var oldVal = store.GetState().val;
                        innerDispatcher(action);
                        var newVal = store.GetState().val;
                        if (newVal != oldVal + 1) {
                            throw new InvalidOperationException($"{oldVal} vs {newVal}");
                        }
                        return action;
                    };
                };
            };
        }

    }

}