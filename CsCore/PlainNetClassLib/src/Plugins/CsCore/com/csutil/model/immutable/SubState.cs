using System;

namespace com.csutil.model.immutable {

    public class SubState<T, S> : IDisposableV2 {

        public IDataStore<T> Store { get; }

        public Func<T, S> SubStateFunc { get; private set; }
        public S State => GetState();

        public Action onStateChanged { get; set; }

        /// <summary> After calling this method, no further state change events will be triggered on this substate </summary>
        public Action RemoveFromParent { get; set; }

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;

        public SubState(IDataStore<T> store, Func<T, S> subStateFunc) {
            Store = store;
            SubStateFunc = subStateFunc;
        }

        public S GetState() {
            ThrowIfDisposed();
            return SubStateFunc(Store.GetState());
        }

        public object Dispatch(object actionToDispatch, bool throwExceptionIfActionDoesNotChangeSubState = true) {
            ThrowIfDisposed();
            return Store.Dispatch(actionToDispatch);
        }

        public void TriggerOnSubstateChanged(S newSubState) {
            ThrowIfDisposed();
            onStateChanged?.Invoke();
        }

        public Action AddStateChangeListener<SubSub>(Func<S, SubSub> getSubSubState, Action<SubSub> onChanged, bool triggerInstantToInit = true) {
            Action newListener = ImmutableExtensions.NewSubstateChangeListener(() => {
                ThrowIfDisposed();
                return getSubSubState(State);
            }, onChanged);
            onStateChanged += newListener;
            if (triggerInstantToInit) { onChanged(getSubSubState(State)); }
            return newListener;
        }

        private void ThrowIfDisposed() {
            if (IsDisposed != DisposeState.Active) { throw new ObjectDisposedException(GetType().Name); }
        }

        /// <summary> If both the substate and the passed in mutable object change (eg because the mutable object is
        /// a child of the substate) then this listener will be triggered </summary>
        public Action AddStateChangeListener<M>(M mutableObj, Action<M> onChanged, bool triggerInstantToInit = true) where M : IsMutable {
            Action newListener = () => {
                ThrowIfDisposed();
                if (StateCompare.WasModifiedInLastDispatch(mutableObj)) { onChanged(mutableObj); }
            };
            onStateChanged += newListener;
            if (triggerInstantToInit) { onChanged(mutableObj); }
            return newListener;
        }

        public void Dispose() {
            IsDisposed = DisposeState.DisposingStarted;
            RemoveFromParent?.Invoke();
            RemoveFromParent = null;
            SubStateFunc = null;
            IsDisposed = DisposeState.Disposed;
        }

    }

}