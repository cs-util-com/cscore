using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;

namespace com.csutil.model.immutable {

    public static class ImmutableExtensions {

        public static Action AddAsyncStateChangeListener<T, S>(this IDataStore<T> s, Func<T, S> getSubState, Func<S, Task> onChanged) {
            return AddStateChangeListener(s, getSubState, (subState) => { onChanged(subState); });
        }

        public static Action AddStateChangeListener<T, S>(this IDataStore<T> self, Func<T, S> getSubState, Action<S> onChanged) {
            Action newListener = NewSubstateChangeListener(() => getSubState(self.GetState()), onChanged);
            self.onStateChanged += newListener;
            return newListener;
        }

        public static SubListeners<S> NewSubStateListener<T, S>(this IDataStore<T> self, Func<T, S> getSubState) {
            var subListener = new SubListeners<S>(getSubState(self.GetState()));
            self.AddStateChangeListener(getSubState, newSubState => { subListener.OnSubstateChanged(newSubState); });
            return subListener;
        }

        public static SubListeners<S> NewSubStateListener<T, S>(this SubListeners<T> self, Func<T, S> getSubState) {
            var subListener = new SubListeners<S>(getSubState(self.latestSubState));
            self.AddStateChangeListener(getSubState, newSubState => { subListener.OnSubstateChanged(newSubState); });
            return subListener;
        }

        internal static Action NewSubstateChangeListener<S>(Func<S> getSubstate, Action<S> onChanged) {
            var oldState = getSubstate();
            Action newListener = () => {
                var newState = getSubstate();
                bool isPrimitive = typeof(S).IsPrimitiveOrSimple();
                if ((!isPrimitive && !ReferenceEquals(oldState, newState)) || (isPrimitive && !Equals(oldState, newState))) {
                    onChanged(newState);
                    oldState = newState;
                }
            };
            return newListener;
        }

        public static ImmutableList<T> MutateEntries<T>(this ImmutableList<T> list, object action, StateReducer<T> reducer) {
            if (list != null) {
                foreach (var elem in list) {
                    var newElem = reducer(elem, action);
                    if (!Object.Equals(newElem, elem)) {
                        list = list.Replace(elem, newElem);
                    }
                }
            }
            return list;
        }

        public static ImmutableDictionary<T, V> MutateEntries<T, V>(this ImmutableDictionary<T, V> list, object action, StateReducer<V> reducer) {
            if (list != null) {
                foreach (var elem in list) {
                    var newValue = reducer(elem.Value, action);
                    if (!Object.Equals(newValue, elem.Value)) {
                        list = list.SetItem(elem.Key, newValue);
                    }
                }
            }
            return list;
        }

        public static ImmutableList<T> AddOrCreate<T>(this ImmutableList<T> self, T t) { return (self == null) ? ImmutableList.Create(t) : self.Add(t); }

        public static T Mutate<T>(this T self, object action, StateReducer<T> reducer, ref bool changed) {
            return self.Mutate<T>(true, action, reducer, ref changed);
        }

        public static T Mutate<T>(this T self, bool applyReducer, object action, StateReducer<T> reducer, ref bool changed) {
            if (!applyReducer) { return self; }
            var newVal = reducer(self, action);
            changed = changed || !Object.Equals(self, newVal);
            AssertValid(self, newVal);
            return newVal;
        }

        /// <summary> Helpfull when the parent object is needed to mutate a field</summary>
        public static T MutateField<P, T>(this P self, T field, object action, FieldReducer<P, T> reducer, ref bool changed) {
            return field.Mutate(action, (previousState, a) => { return reducer(self, previousState, a); }, ref changed);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")] // Stripped from production code
        private static void AssertValid<T>(T oldVal, T newVal) {
            if (oldVal is IsValid v1) {
                AssertV2.IsTrue(v1.IsValid(), "Old value before mutation invalid");
            }
            if (!Object.Equals(oldVal, newVal) && newVal is IsValid v2) {
                AssertV2.IsTrue(v2.IsValid(), "New value after mutation invalid");
            }
        }

        public static ForkedStore<T> NewFork<T>(this DataStore<T> s) {
            return new ForkedStore<T>(s, s.reducer);
        }

    }

    /// <summary> Similar to the StateReducer but provides the parent context of the field as well </summary>
    /// /// <param name="action"> The action to be applied to the state tree. </param>
    /// /// <returns> The new field value. </returns>
    public delegate T FieldReducer<P, T>(P parent, T oldFieldValue, object action);

    /// <summary> ts a method that is used to update the state tree. </summary>
    /// <param name="action"> The action to be applied to the state tree. </param>
    /// <returns> The updated state tree. </returns>
    public delegate T StateReducer<T>(T previousState, object action);

    /// <summary> Represents a method that dispatches an action. </summary>
    /// <param name="action"> The action to dispatch. </param>
    public delegate object Dispatcher(object action);

    /// <summary> Represents a method that is used as middleware. </summary>
    /// <typeparam name="T">  The state tree type. </typeparam>
    /// <returns> A function that, when called with a <see cref="Dispatcher" />, returns a new <see cref="Dispatcher" /> that wraps the first one. </returns>
    public delegate Func<Dispatcher, Dispatcher> Middleware<T>(IDataStore<T> store);

    public class SubListeners<SubState> {

        public Action innerListeners;
        public SubState latestSubState { get; private set; }
        public SubListeners(SubState currentSubState) { latestSubState = currentSubState; }

        public void OnSubstateChanged(SubState newSubState) {
            latestSubState = newSubState;
            innerListeners.InvokeIfNotNull();
        }

        public Action AddStateChangeListener<SSS>(Func<SubState, SSS> getSubSubState, Action<SSS> onChanged) {
            Action newListener = ImmutableExtensions.NewSubstateChangeListener(() => getSubSubState(latestSubState), onChanged);
            innerListeners += newListener;
            return newListener;
        }

    }

}