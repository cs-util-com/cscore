using System;

namespace com.csutil.model.immutable {

    public class SubState<T, S> : IDisposableV2 {

        public readonly IDataStore<T> Store;

        public readonly Func<S> GetState;

        private Action _onStateChanged;
        public Action onStateChanged {
            get => _onStateChanged;
            set {
                RegisterInStoreIfNeeded(value);
                _onStateChanged = value;
            }
        }

        /// <summary> If someone subscribes to the substate but the substate itself is not yet subscribed to
        /// any parent substate or the store itself, then register the substate with the store </summary>
        private void RegisterInStoreIfNeeded(Action value) {
            if (value != null && this.IsDisposed == DisposeState.Active) {
                Store.AddSubStateAsListener(this);
            }
        }

        /// <summary> After calling this method, no further state change events will be triggered on this substate </summary>
        public Action RemoveFromParent { get; set; }

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;

        public SubState(IDataStore<T> store, Func<T, S> subStateFunc) {
            Store = store;
            GetState = () => {
                this.ThrowErrorIfDisposed();
                return subStateFunc(Store.GetState());
            };
        }

        public object Dispatch(object actionToDispatch, bool throwExceptionIfActionDoesNotChangeSubState = true) {
            this.ThrowErrorIfDisposed();
            return Store.Dispatch(actionToDispatch);
        }

        public void TriggerOnSubstateChanged(S newSubState) {
            this.ThrowErrorIfDisposed();
            onStateChanged?.Invoke();
        }

        public Action AddStateChangeListener(Action<S> onChanged, bool triggerInstantToInit = true) {
            Action newListener = ImmutableExtensions.NewSubstateChangeListener(() => {
                this.ThrowErrorIfDisposed();
                return GetState();
            }, onChanged);
            onStateChanged += newListener;
            if (triggerInstantToInit) { onChanged(GetState()); }
            return newListener;
        }
        
        public void RemoveStateChangeListener(Action onChanged) {
            onStateChanged -= onChanged;
        }

        public Action AddStateChangeListener<SubSub>(Func<S, SubSub> getSubSubState, Action<SubSub> onChanged, bool triggerInstantToInit = true) {
            Action newListener = ImmutableExtensions.NewSubstateChangeListener(() => {
                this.ThrowErrorIfDisposed();
                return getSubSubState(GetState());
            }, onChanged);
            onStateChanged += newListener;
            if (triggerInstantToInit) { onChanged(getSubSubState(GetState())); }
            return newListener;
        }

        /// <summary> If both the SubState and the passed in mutable object change (eg because the mutable object is
        /// a child of the SubState) then this listener will be triggered </summary>
        public Action AddStateChangeListener<M>(M mutableObj, Action<M> onChanged, bool triggerInstantToInit = true) where M : IsMutable {
            Action newListener = () => {
                this.ThrowErrorIfDisposed();
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
            IsDisposed = DisposeState.Disposed;
        }

    }

}