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
        [Obsolete("Use store.AddStoreMutationEventBroadcaster() instead")]
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

        /// <summary> Broadcasts all actions on the EventBus that are dispatched to the store </summary>
        public static Action AddStoreMutationEventBroadcaster<T>(this IDataStore<T> store) {
            return store.AddStateChangeListener(x => x, delegate {
                object action = store.LastDispatchedAction;
                EventBus.instance.Publish(EventConsts.catMutation, "" + action, action);
            }, triggerInstantToInit: false);
        }

        public static Middleware<T> NewLoggingMiddleware<T>(bool showStateDiff = true, int maxMsBudgetForLoggingChanges = 1000) {
            return (store) => {
                return (Dispatcher innerDispatcher) => {
#if !DEBUG
                    return innerDispatcher;
#endif
                    return NewLoggingDispatcher(store, innerDispatcher, maxMsBudgetForLoggingChanges, showStateDiff);
                };
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

        private static Dispatcher NewLoggingDispatcher<T>(IDataStore<T> store, Dispatcher innerDispatcher, int maxMsBudgetForLoggingChanges, bool showStateDiff) {
            var showChangesJson = true;
            return (action) => {
                if (action is IsValid v && !v.IsValid()) {
                    Log.e("Invalid action: " + asJson(action.GetType().Name, action));
                }

                if (!showChangesJson || (action is IDontLogDispatch)) {
                    return innerDispatcher(action);
                }

                bool copyOfActionSupported = false;
                object actionBeforeDispatch = null;
                MakeDebugCopyOfAction(action, ref copyOfActionSupported, ref actionBeforeDispatch);

                T previousState = store.GetState();
                var returnedAction = innerDispatcher(action);
                T newState = store.GetState();

                if (copyOfActionSupported) { AssertActionDidNotChangeDuringDispatch(actionBeforeDispatch, action); }

                if (!StateCompare.WasModified(previousState, newState)) {
                    Log.w($"The action was not handled by any of the reducers of Store<{store.GetState().GetType().Name}>:"
                        + "\n" + asJson("" + action.GetType().Name, action));
                } else {
                    var t = StopwatchV2.StartNewV2("NewLoggingMiddleware->NewLoggingDispatcher");
                    ShowChanges(action, previousState, newState, showStateDiff);
                    t.StopV2();
                    if (t.ElapsedMilliseconds > maxMsBudgetForLoggingChanges) {
                        showChangesJson = false;
                        Log.e("Disabling logging of json diff from store mutation, since it became to slow (store content to large): " + t);
                    }
                }
                return returnedAction;
            };
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_FULL_LOGGING")]
        public static void MakeDebugCopyOfAction(object action, ref bool copyOfActionSupported, ref object actionBeforeDispatch) {
            try {
                // If the action is a delegate it will be handled by thunk and cant be copied:
                if (action is Delegate) { return; }
                // Make a deep copy of the action 
                actionBeforeDispatch = action.DeepCopyViaTypedJson();
                if (actionBeforeDispatch != null) {
                    copyOfActionSupported = !HasDiff(actionBeforeDispatch, action, out var diff);
                    if (!copyOfActionSupported) {
                        Log.w("JSON copy not supported for action: " + action + "! Unhandled JSON fields: \n" + diff);
                    }
                }
            } catch (Exception e) {
                Log.w("Failed to do a JSON copy of action: " + action + "\n", e);
            }
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
            diff = MergeJson.GetDiff(actionAfter, actionBeforeDispatch);
            return !diff.IsNullOrEmpty();
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_FULL_LOGGING")]
        private static void ShowChanges<T>(object action, T previousState, T newState, bool showStateDiff) {
            try {
                Log.d(asJson("" + action.GetType().Name, action));
                if (showStateDiff) {
                    JToken diff = MergeJson.GetDiff(previousState, newState);
                    Log.d(asJson("previousState -> newState diff", diff));
                }
            } catch (Exception e) { Log.e(e); }
        }

        private static string asJson(string varName, object result) { return varName + "=" + JsonWriter.AsPrettyString(result); }

        /// <summary> Can be implemented by an action to prevent any default logging by the default logger middleware for this action </summary>
        public class IDontLogDispatch {
        }

        public static Middleware<SlicedModel> NewSliceChangeHandler<T>(Action<IDataStore<SlicedModel>, T, object, T> onSliceChanged) {
            return (IDataStore<SlicedModel> store) => {
                return (Dispatcher innerDispatcher) => {
                    return (object action) => {
                        store.GetState().TryGetSlice<T>(out var oldSlice);
                        var a = innerDispatcher(action);
                        store.GetState().TryGetSlice<T>(out var newSlice);
                        if (StateCompare.WasModified(oldSlice, newSlice)) {
                            onSliceChanged(store, oldSlice, action, newSlice);
                        }
                        return a;
                    };
                };
            };
        }

    }

}