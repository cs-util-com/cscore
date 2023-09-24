using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.integrationTests.model.immutable {
    
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