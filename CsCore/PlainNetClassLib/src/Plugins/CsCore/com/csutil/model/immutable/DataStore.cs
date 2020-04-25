using System;

namespace com.csutil.model.immutable {

    /// <summary> 
    /// Represents a store that encapsulates a state tree and is used to dispatch actions to update the state tree. 
    /// The store follows the main conventions and syntax of the Redux store and it can be used in combination
    /// with the provided middlewares for logging, async actions undo/redo etc. </summary>
    /// <typeparam name="T"> The state tree type. </typeparam>
    public class DataStore<T> : IDataStore<T> {

        private readonly object threadLock = new object();
        private readonly Dispatcher dispatcher;
        private T state;

        public string storeName;
        public readonly StateReducer<T> reducer;
        public Action onStateChanged { get; set; }

        public DataStore(StateReducer<T> reducer, T initialState = default(T), params Middleware<T>[] middlewares) {
            this.state = initialState;
            this.reducer = reducer;
            dispatcher = ApplyMiddlewares(middlewares);
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

        public override string ToString() {
            if (storeName != null) { return storeName; }
            return base.ToString();
        }

        public void Destroy() {
            state = default(T);
            onStateChanged = null;
            storeName = "DESTROYED " + storeName;
        }

    }

}