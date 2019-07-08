using System;
using com.csutil.model.store;
using Xunit;

namespace com.csutil.tests.model.store {

    public class DataStoreTests {

        public DataStoreTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        public class MyAppState1 {
            public int counter = 0;
            public MyUser1 user;

        }

        public class MyUser1 { public string name = ""; }

        private class IncreaseCounterAction { public int amount; }

        private class ActionChangeUserName { public string newName; }

        [Fact]
        public void ExampleUsage1() {

            var data = new MyAppState1();
            data.user = new MyUser1();

            var s = new DataStore<MyAppState1>(MyReducers1.ReduceMyAppState1, data, loggingMiddleware);

            s.Dispatch(new IncreaseCounterAction() { amount = 2 });
            s.Dispatch(new ActionChangeUserName() { newName = "Carl" });
            s.Dispatch(new IncreaseCounterAction() { amount = 4 });

            Assert.Equal(6, s.GetState().counter);
            Assert.Equal("Carl", s.GetState().user.name);

        }

        private Func<Dispatcher, Dispatcher> loggingMiddleware(DataStore<MyAppState1> store) {
            Log.MethodEntered("store=" + store);
            return (Dispatcher dispatcher) => {
                Log.MethodEntered("apply middleWare1", dispatcher);
                Dispatcher wrapperDispatcher = (action) => {
                    var previousState = store.GetState();
                    var returnedAction = dispatcher(action);
                    var newState = store.GetState();
                    Log.MethodEntered(
                        asJson("previousState", previousState),
                        asJson("action", action),
                        asJson("newState", newState));
                    return returnedAction;
                };
                return wrapperDispatcher;
            };
        }

        private static string asJson(string varName, object result) { return varName + "=" + JsonWriter.AsPrettyString(result); }

        public static class MyReducers1 {

            public static MyAppState1 ReduceMyAppState1(MyAppState1 previousState, object action) {
                return new MyAppState1() {
                    counter = ReduceCounter(previousState.counter, action),
                    user = ReduceUser(previousState.user, action)
                };
            }

            private static int ReduceCounter(int previousState, object action) {
                if (action is IncreaseCounterAction ica) { return previousState + ica.amount; }
                return previousState;
            }

            private static MyUser1 ReduceUser(MyUser1 previousState, object action) {
                return new MyUser1() {
                    name = ReduceName(previousState.name, action)
                };
            }

            private static string ReduceName(string previousState, object action) {
                if (action is ActionChangeUserName a) { return a.newName; }
                return previousState;
            }

        }

    }
}