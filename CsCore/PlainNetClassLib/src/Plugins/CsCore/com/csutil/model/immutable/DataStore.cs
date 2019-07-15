using System;

namespace com.csutil.model.immutable {

    /// <summary> Represents a store that encapsulates a state tree and is used to dispatch actions to update the state tree. </summary>
    /// <typeparam name="T"> The state tree type. </typeparam>
    public class DataStore<T> {

        private readonly object threadLock = new object();
        private readonly Dispatcher dispatcher;
        private readonly StateReducer<T> reducer;
        private T state;
        public Action onStateChanged;

        public DataStore(StateReducer<T> reducer, T initialState = default(T), params Middleware<T>[] middlewares) {
            this.reducer = reducer;
            dispatcher = ApplyMiddlewares(middlewares);
            state = initialState;
        }

        private Dispatcher ApplyMiddlewares(params Middleware<T>[] middlewares) {
            Dispatcher dispatcher = (object action) => {
                lock (threadLock) { state = reducer(state, action); }
                onStateChanged?.Invoke();
                return action;
            };
            foreach (var middleware in middlewares) { dispatcher = middleware(this)(dispatcher); }
            return dispatcher;
        }

        /// <summary> Dispatches an action to the store. </summary>
        /// <param name="action"> The action to dispatch. </param>
        /// <returns> Varies depending on store enhancers. With no enhancers Dispatch returns the action that was passed to it. </returns>
        public object Dispatch(object action) { return dispatcher(action); }

        /// <summary> Gets the current state tree. </summary>
        /// <returns>  The current state tree. </returns>
        public T GetState() { return state; }

    }

    /// <summary> Represents a method that dispatches an action. </summary>
    /// <param name="action"> The action to dispatch. </param>
    public delegate object Dispatcher(object action);

    /// <summary> Represents a method that is used as middleware. </summary>
    /// <typeparam name="T">  The state tree type. </typeparam>
    /// <returns> A function that, when called with a <see cref="Dispatcher" />, returns a new <see cref="Dispatcher" /> that wraps the first one. </returns>
    public delegate Func<Dispatcher, Dispatcher> Middleware<T>(DataStore<T> store);

    /// <summary> ts a method that is used to update the state tree. </summary>
    /// <param name="action"> The action to be applied to the state tree. </param>
    /// <returns> The updated state tree. </returns>
    public delegate T StateReducer<T>(T previousState, object action);

}