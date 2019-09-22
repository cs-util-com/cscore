using System;
using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;

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
                Log.MethodEntered("NewLoggingMiddleware", "store =" + store);
                return (Dispatcher innerDispatcher) => {
                    Dispatcher loggingDispatcher = (action) => {
                        if (action is IsValid v && !v.IsValid()) {
                            Log.e("Invalid action: " + asJson(action.GetType().Name, action));
                        }
                        T previousState = store.GetState();
                        var returnedAction = innerDispatcher(action);
                        T newState = store.GetState();
                        if (Object.Equals(previousState, newState)) {
                            Log.w("The action  " + action + " was not handled by any of the reducers! Store=" + store);
                        } else {
                            ShowChanges(action, previousState, newState);
                        }
                        return returnedAction;
                    };
                    return loggingDispatcher;
                };
            };
        }

        private static void ShowChanges<T>(object action, T previousState, T newState) {
            try {
                JToken diff = MergeJson.GetDiff(previousState, newState);
                Log.d(asJson("" + action.GetType().Name, action), asJson("previousState -> newState diff", diff));
            } catch (Exception e) { Log.e(e); }
        }

        private static string asJson(string varName, object result) { return varName + "=" + JsonWriter.AsPrettyString(result); }


    }

}