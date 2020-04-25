using System.Collections.Generic;
using System.Linq;

namespace com.csutil.model.immutable {

    /// <summary>
    /// A store that can be used to temporarely fork from a source store to do changes locally 
    /// that can be discarded easily in case the user decides to cancel a process. E.g. when 
    /// the user enters information in a form but decides to cancel the interaction. When using 
    /// a ForkedStore the already dispatched actions do not have to be reverted via Undo since the
    /// changes are only applied to the forked store at this point.
    /// </summary>
    public class ForkedStore<T> : DataStore<T> {

        public List<object> recordedActions { get; }
        public IDataStore<T> forkedStore { get; }

        public ForkedStore(IDataStore<T> storeToFork, StateReducer<T> reducer, params Middleware<T>[] middlewares)
                : this(reducer, storeToFork.GetState(), new List<object>(), middlewares) {
            this.forkedStore = storeToFork;
        }

        private ForkedStore(StateReducer<T> reducer, T initialState, List<object> actions, params Middleware<T>[] middlewares)
            : base(reducer, initialState, AddCollectMiddleWare(actions, middlewares)) {
            this.recordedActions = actions;
        }

        public void ApplyMutationsBackToOriginalStore() {
            foreach (var action in recordedActions) { forkedStore.Dispatch(action); }
        }

        private static Middleware<T>[] AddCollectMiddleWare(List<object> actions, Middleware<T>[] middlewares) {
            Middleware<T> collecterMiddleware = NewCollectActionMiddleware(actions);
            return middlewares.AddViaUnion(collecterMiddleware).ToArray();
        }

        private static Middleware<T> NewCollectActionMiddleware(List<object> actions) {
            return (IDataStore<T> store) => {
                return (Dispatcher innerDispatcher) => {
                    Dispatcher outerDispatcher = (action) => {
                        actions.Add(action);
                        return innerDispatcher(action);
                    };
                    return outerDispatcher;
                };
            };
        }

    }

}
