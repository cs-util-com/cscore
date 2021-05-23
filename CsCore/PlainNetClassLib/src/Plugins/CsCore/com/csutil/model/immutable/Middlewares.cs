using System;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace com.csutil.model.immutable {
    public static class Middlewares {

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

        /// <summary> This middleware will automatically log all dispatched actions to the store to the AppFlow logic to track them there </summary>
        public static Middleware<T> NewMutationBroadcasterMiddleware<T>(object extraArgument = null) {
            return (IDataStore<T> store) => {
                return (Dispatcher innerDispatcher) => {
                    Dispatcher outerDispatcher = (action) => {
                        EventBus.instance.Publish(EventConsts.catMutation, "" + action, action);
                        return innerDispatcher(action);
                    };
                    return outerDispatcher;
                };
            };
        }

        public static Middleware<T> NewLoggingMiddleware<T>() {
            return (store) => {
                using (Log.MethodEntered("NewLoggingMiddleware", "store =" + store)) {
                    return (Dispatcher innerDispatcher) => {
#if !DEBUG
                        return innerDispatcher;
#endif
                        return NewLoggingDispatcher(store, innerDispatcher);
                    };
                }
            };
        }

        public static Middleware<T> NewMutableDataSupport<T>() {
            return (IDataStore<T> store) => {
                return (Dispatcher innerDispatcher) => {
                    Dispatcher dispatcher = (action) => {
                        StateCompare.SetStoreDispatchingStarted();
                        var a = innerDispatcher(action);
                        StateCompare.SetStoreDispatchingEnded();
                        return a;
                    };
                    return dispatcher;
                };
            };
        }

        private static Dispatcher NewLoggingDispatcher<T>(IDataStore<T> store, Dispatcher innerDispatcher) {
            return (action) => {
                if (action is IsValid v && !v.IsValid()) {
                    Log.e("Invalid action: " + asJson(action.GetType().Name, action));
                }

                bool copyOfActionSupported = false;
                object actionBeforeDispatch = null;
                MakeDebugCopyOfAction(action, ref copyOfActionSupported, ref actionBeforeDispatch);

                T previousState = store.GetState();
                var returnedAction = innerDispatcher(action);
                T newState = store.GetState();

                if (copyOfActionSupported) { AssertActionDidNotChangeDuringDispatch(actionBeforeDispatch, action); }

                if (!StateCompare.WasModified(previousState, newState)) {
                    Log.w("The action  " + action + " was not handled by any of the reducers! Store=" + store);
                } else {
                    ShowChanges(action, previousState, newState);
                }
                return returnedAction;
            };
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_FULL_LOGGING")]
        private static void MakeDebugCopyOfAction(object action, ref bool copyOfActionSupported, ref object actionBeforeDispatch) {
            try {
                // If the action is a delegate it will be handled by thunk and cant be copied:
                actionBeforeDispatch = action is Delegate ? null : action.DeepCopyViaTypedJson();
                if (actionBeforeDispatch != null) {
                    copyOfActionSupported = !HasDiff(actionBeforeDispatch, action, out _);
                    if (!copyOfActionSupported) { Log.w("Deep copy not supported for action: " + action); }
                }
            }
            catch (Exception) { }
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_FULL_LOGGING")]
        private static void AssertActionDidNotChangeDuringDispatch(object actionBeforeDispatch, object actionAfter) {
            if (HasDiff(actionBeforeDispatch, actionAfter, out JToken diff)) {
                Log.e($"The action {actionAfter.GetType()} was changed by dispatching it, check reducers: "
                    + "\n Diff: " + diff.ToPrettyString()
                    + "\n\n Action before dispatch: " + JsonWriter.AsPrettyString(actionBeforeDispatch)
                    + "\n\n Action after dispatch: " + JsonWriter.AsPrettyString(actionAfter)
                    );
            }
        }

        private static bool HasDiff(object actionBeforeDispatch, object actionAfter, out JToken diff) {
            diff = MergeJson.GetDiff(actionBeforeDispatch, actionAfter);
            return !diff.IsNullOrEmpty();
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_FULL_LOGGING")]
        private static void ShowChanges<T>(object action, T previousState, T newState) {
            try {
                JToken diff = MergeJson.GetDiff(previousState, newState);
                Log.d(asJson("" + action.GetType().Name, action), asJson("previousState -> newState diff", diff));
            }
            catch (Exception e) { Log.e(e); }
        }

        private static string asJson(string varName, object result) { return varName + "=" + JsonWriter.AsPrettyString(result); }

    }

}