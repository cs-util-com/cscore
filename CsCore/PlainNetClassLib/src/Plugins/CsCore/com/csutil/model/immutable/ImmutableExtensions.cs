using System;
using System.Collections.Immutable;

namespace com.csutil.model.immutable {

    public static class ImmutableExtensions {

        public static Action AddStateChangeListener<T, S>(this IDataStore<T> s, Func<T, S> getSubState, Action<S> onChanged) {
            var oldState = getSubState(s.GetState());
            Action newListener = () => {
                var newState = getSubState(s.GetState());
                if (!Object.ReferenceEquals(oldState, newState)) {
                    onChanged(newState);
                    oldState = newState;
                }
            };
            s.onStateChanged += newListener;
            return newListener;
        }

        public static ImmutableList<T> MutateEntries<T>(this ImmutableList<T> list, object action, StateReducer<T> reducer) {
            if (list != null) {
                list.ForEach(elem => {
                    var newElem = reducer(elem, action);
                    if (!Object.Equals(newElem, elem)) { list = list.Replace(elem, newElem); }
                });
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
            return newVal;
        }

    }

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

}