using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace com.csutil.model.immutable {

    public class StateCompare {

        private static StateCompare instance = new StateCompare();

        /// <summary> The nr that represents the time of the last dispatch </summary>
        private long lastDispatchStart;
        /// <summary> The nr that represents the time of the last dispatch </summary>
        private long lastDispatchEnd;

        /// <summary> This has to be called by a store middleware  for every new dispatch if a 
        /// store should support mutable data. </summary>
        public static void SetStoreDispatchingStarted() {
            instance.lastDispatchStart = Stopwatch.GetTimestamp();
        }

        public static void SetStoreDispatchingEnded() {
            instance.lastDispatchEnd = Stopwatch.GetTimestamp();
        }

        public static bool IsCurrentlyDispatching() {
            return instance.lastDispatchStart > instance.lastDispatchEnd;
        }

        public static bool WasModified<S>(S oldState, S newState) {
            if (oldState == null && newState == null) { return false; }
            if (oldState != null && oldState.GetType().IsPrimitiveOrSimple()) { return !Equals(oldState, newState); }
            if (newState != null && newState.GetType().IsPrimitiveOrSimple()) { return !Equals(oldState, newState); }
            if (typeof(S).IsKeyValuePairType()) {
                try { return WasKeyValuePairModified(oldState, newState); } catch (Exception e) {
                    Log.w("Accessing KeyValuePair failed: " + e);
                    Debugger.Break();
                }
            }
            if (Nullable.GetUnderlyingType(typeof(S)) != null) { return !Equals(oldState, newState); }
            if (!ReferenceEquals(oldState, newState)) { return true; }
            if (oldState is IsMutable m) { return WasModifiedInLastDispatch(m); }
            return false;
        }

        private static bool WasKeyValuePairModified<S>(S oldState, S newState) {
            var keyProp = typeof(S).GetProperty("Key");
            var valueProp = typeof(S).GetProperty("Value");
            var oldKey = keyProp.GetValue(oldState, null);
            var newKey = keyProp.GetValue(newState, null);
            var oldValue = valueProp.GetValue(oldState, null);
            var newValue = valueProp.GetValue(newState, null);
            AssertV3.IsTrue(Equals(oldKey, newKey),
                () => $"Key of KeyValuePair must not change, oldKey={oldKey} newKey={newKey}");
            return !Equals(oldKey, newKey) || WasModified(oldValue, newValue);
        }

        public static bool WasModifiedInLastDispatch(IsMutable mutableData) {
            return instance.lastDispatchStart < mutableData.LastMutation;
        }

    }

    /// <summary> Has to be implemented by any mutable data model that is stored in a data store </summary>
    public interface IsMutable {
        /// <summary> A tick value (not a timestamp!) when the instance was last mutated, should 
        /// only be modified by calling object.MarkMutated() </summary>
        long LastMutation { get; set; }
    }

    public static class IsMutableExtensions {

        /// <summary> This should only be used in the datastore dispatchers, marks that 
        /// the object was modified by the dispatched mutation </summary>
        public static void MarkMutated(this IsMutable self) {
            if (!StateCompare.IsCurrentlyDispatching()) {
                throw new InvalidOperationException("Model was modified outside of the dispatchers");
            }
            self.LastMutation = Stopwatch.GetTimestamp();
        }

        /// <summary> Will be true if the object was modified when the last mutation was 
        /// dispatched via the data store the object is managed in </summary>
        public static bool WasModifiedInLastDispatch(this IsMutable self) {
            return StateCompare.WasModifiedInLastDispatch(self);
        }

    }

}