using System;
using Newtonsoft.Json;

namespace com.csutil.model.immutable {

    public class SubState<T, S> {

        public IDataStore<T> Store { get; }

        public Func<T, S> GetSubState { get; }
        public S State => GetSubState(Store.GetState());

        private Action _innerListeners;
        
        public SubState(IDataStore<T> store, Func<T, S> getSubState) {
            Store = store;
            GetSubState = getSubState;
        }

        public object Dispatch(object actionToDispatch) {
            var store = Store;
            var oldStateInStore = GetSubState(store.GetState());
            var result = store.Dispatch(actionToDispatch);
            var newStateInStore = GetSubState(store.GetState());
            if (ReferenceEquals(oldStateInStore, newStateInStore)) {
                throw new InvalidOperationException("Dispatching the action on the entity did not cause the entity to change, actionToDispatch=" + actionToDispatch);
            }
            return result;
        }

        public Action AddStateChangeListener<T>(Func<S, T> getSubSubState, Action<T> onChanged, bool triggerInstantToInit = true) {
            Action newListener = ImmutableExtensions.NewSubstateChangeListener(() => getSubSubState(State), onChanged);
            _innerListeners += newListener;
            if (triggerInstantToInit) { onChanged(getSubSubState(State)); }
            return newListener;
        }
        
        public void OnSubstateChanged(S newSubState) {
            _innerListeners.InvokeIfNotNull();
        }

        // public Action AddStateChangeListener<S>(S mutableObj, Action<S> onChanged, bool triggerInstantToInit = true) where S : IsMutable {
        //     Action newListener = () => {
        //         if (StateCompare.WasModifiedInLastDispatch(mutableObj)) { onChanged(mutableObj); }
        //     };
        //     _innerListeners += newListener;
        //     if (triggerInstantToInit) { onChanged(mutableObj); }
        //     return newListener;
        // }
        

    }

}