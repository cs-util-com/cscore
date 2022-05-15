using System;
using System.Threading.Tasks;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    public class StoreMiddlewareExecutionOrderTests {

        public StoreMiddlewareExecutionOrderTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void TestDispatchInListenerCallback() {

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

        [Fact]
        public void TestDispatchInMiddleware() {
            DataStore<MyModel> store = null;
            bool flag1 = true;
            bool flag2 = false;
            // Simulate that the middleware for some events Dispatches actions itself:
            var middleware = NewMiddlewareThatDispatchesEvents(() => {
                if (flag1) {
                    flag1 = false;
                    store.Dispatch(new MyAction() { val = 2 });
                }
                if (flag2) {
                    flag2 = false;
                    store.Dispatch(new MyAction() { val = 4 });
                }
            });
            store = new DataStore<MyModel>(ReduceMyModel, new MyModel(0), middleware);

            int counter = 0;
            { // Ensure that in the listener never an older state arrives aver a newer state:
                int val = store.GetState().val;
                store.AddStateChangeListener(x => x.val, newVal => {
                    Log.d($"{val} => {newVal}");
                    counter++;
                    if (val >= newVal) {
                        throw new InvalidOperationException($"{val} vs {newVal}");
                    }
                    val = newVal;
                }, triggerInstantToInit: false);
            }

            store.Dispatch(new MyAction() { val = 1 });
            Assert.Equal(2, store.GetState().val);
            // Even though there are 2 actions dispatched the listener is only triggered once since the
            // state does not change between the first and second listener call. Both actions already have
            // been send through the Reducers:
            Assert.Equal(1, counter);

            flag2 = true;
            store.Dispatch(new MyAction() { val = 3 });
            Assert.Equal(4, store.GetState().val);
            Assert.Equal(2, counter);
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
                    throw new InvalidOperationException($"ReduceMyModel: {previousState.val} vs {a.val}");
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

        private Middleware<MyModel> NewMiddlewareThatDispatchesEvents(Action onDispatch) {
            int val = 0;
            return (store) => {
                return (innerDispatcher) => {
                    return (action) => {
                        innerDispatcher(action);
                        if (val + 1 != store.GetState().val) {
                            throw new InvalidOperationException($"{val} vs {store.GetState().val}");
                        }
                        val = store.GetState().val;
                        onDispatch.InvokeIfNotNull();
                        return action;
                    };
                };
            };
        }

    }

}