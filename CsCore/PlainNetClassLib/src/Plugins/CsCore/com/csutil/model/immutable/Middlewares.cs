using System;

namespace com.csutil.model.immutable {
    public class Middlewares {

        // See e.g. https://github.com/reduxjs/redux-thunk/blob/master/src/index.js
        public static Middleware<T> NewThunkMiddleware<T>(object extraArgument = null) {
            return (IDataStore<T> store) => {
                return (Dispatcher innerDispatcher) => {
                    Dispatcher thunkDispatcher = (action) => {
                        if (action is Delegate d) { return d.DynamicInvokeV2(store, extraArgument); }
                        return innerDispatcher(action);
                    };
                    return thunkDispatcher;
                };
            };
        }

        public static Middleware<T> NewLoggingMiddleware<T>() {
            return (store) => {
                Log.MethodEntered("store=" + store);
                return (Dispatcher innerDispatcher) => {
                    Dispatcher loggingDispatcher = (action) => {
                        T previousState = store.GetState();
                        var returnedAction = innerDispatcher(action);
                        T newState = store.GetState();
                        if (Object.Equals(previousState, newState)) {
                            Log.w("The action  " + action + " was not handled by any of the reducers! Store=" + store);
                        } else {
                            Log.d(asJson("previousState", previousState), asJson("" + action.GetType(), action), asJson("newState", newState));
                        }
                        return returnedAction;
                    };
                    return loggingDispatcher;
                };
            };
        }

        private static string asJson(string varName, object result) { return varName + "=" + JsonWriter.AsPrettyString(result); }


    }

}