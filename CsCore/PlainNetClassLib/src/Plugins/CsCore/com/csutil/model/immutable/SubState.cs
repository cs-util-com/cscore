using System;
using Newtonsoft.Json;

namespace com.csutil.model.immutable {

    public class SubState<T, S> {

        public IDataStore<T> Store { get; }

        public Func<T, S> SubStateFunc { get; }
        public S State => SubStateFunc(Store.GetState());

        public Action<S> onStateChanged { get; set; }

        public SubState(IDataStore<T> store, Func<T, S> subStateFunc) {
            Store = store;
            SubStateFunc = subStateFunc;
        }

        public object Dispatch(object actionToDispatch, bool throwExceptionIfActionDoesNotChangeSubState = true) {
            var store = Store;
            var oldStateInStore = SubStateFunc(store.GetState());
            var result = store.Dispatch(actionToDispatch);
            var newStateInStore = SubStateFunc(store.GetState());
            if (throwExceptionIfActionDoesNotChangeSubState && !StateCompare.WasModified(oldStateInStore, newStateInStore)) {
                throw new InvalidOperationException("Dispatching the action on the entity did not cause the entity to change, actionToDispatch=" + actionToDispatch);
            }
            return result;
        }

        public void OnSubstateChanged(S newSubState) {
            onStateChanged?.Invoke(newSubState);
        }

        public Action<S> AddStateChangeListener<SubSub>(Func<S, SubSub> getSubSubState, Action<SubSub> onChanged, bool triggerInstantToInit = true) {
            Action<S> newListener = (s) => ImmutableExtensions.NewSubstateChangeListener(() => getSubSubState(State), onChanged);
            onStateChanged += newListener;
            if (triggerInstantToInit) { onChanged(getSubSubState(State)); }
            return newListener;
        }

        /// <summary> If both the substate and the passed in mutable object change (eg because the mutable object is
        /// a child of the substate) then this listener will be triggered </summary>
        public Action<S> AddStateChangeListener<M>(M mutableObj, Action<M> onChanged, bool triggerInstantToInit = true) where M : IsMutable {
            Action<S> newListener = (s) => {
                if (StateCompare.WasModifiedInLastDispatch(mutableObj)) { onChanged(mutableObj); }
            };
            onStateChanged += newListener;
            if (triggerInstantToInit) { onChanged(mutableObj); }
            return newListener;
        }

    }

}