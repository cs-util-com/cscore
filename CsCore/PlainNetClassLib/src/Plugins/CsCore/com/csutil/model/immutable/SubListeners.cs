using System;

namespace com.csutil.model.immutable {
    
    [Obsolete("Use SubState class instead")]
    public class SubListeners<SubState> {

        public Action innerListeners;

        /// <summary> This is the action that can be called to unregister the <see cref="SubListeners{SubState}"/> in its parent again </summary>
        private Action unregisterInParentAction;

        public SubState latestSubState { get; private set; }
        public SubListeners(SubState currentSubState) { latestSubState = currentSubState; }

        public void OnSubstateChanged(SubState newSubState) {
            latestSubState = newSubState;
            innerListeners.InvokeIfNotNull();
        }

        public Action AddStateChangeListener<T>(Func<SubState, T> getSubSubState, Action<T> onChanged, bool triggerInstantToInit = true) {
            Action newListener = ImmutableExtensions.NewSubstateChangeListener(() => getSubSubState(latestSubState), onChanged);
            innerListeners += newListener;
            if (triggerInstantToInit) { onChanged(getSubSubState(latestSubState)); }
            return newListener;
        }

        public Action AddStateChangeListener<S>(S mutableObj, Action<S> onChanged, bool triggerInstantToInit = true) where S : IsMutable {
            Action newListener = () => {
                if (StateCompare.WasModifiedInLastDispatch(mutableObj)) { onChanged(mutableObj); }
            };
            innerListeners += newListener;
            if (triggerInstantToInit) { onChanged(mutableObj); }
            return newListener;
        }

        public void SetUnregisterInParentAction(Action unregisterInParentAction) {
            this.unregisterInParentAction = unregisterInParentAction;
        }

        /// <summary> Can be called to unregister the <see cref="SubListeners{SubState}"/> in its parent again. Afterwards it (and all its children) will no longer be informed about updates </summary>
        public void UnregisterFromParent() { unregisterInParentAction(); }

    }
    
}