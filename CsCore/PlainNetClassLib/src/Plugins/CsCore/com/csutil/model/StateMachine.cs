using System;
using System.Collections.Generic;

namespace com.csutil {
    public static class StateMachine {

        private const string TRANSITION_EVENT = "StateMachineTransition_";

        /// <summary> 
        /// Uses the map of allowed transitions to evaluate the transition request and throws
        /// an exception its not valid.
        /// </summary>
        public static T TransitionTo<T>(this Dictionary<T, HashSet<T>> allowedTransitions, T currentState, T newState) {
            if (!allowedTransitions.ContainsKey(currentState)) {
                throw Log.e(currentState + " -> " + newState + " blocked, " + currentState + " has no allowed transitions");
            } else if (!allowedTransitions[currentState].Contains(newState)) {
                throw Log.e(currentState + " -> " + newState + " blocked, transition not allowed!");
            }
            EventBus.instance.Publish(TRANSITION_EVENT + (typeof(T)), currentState, newState);
            return newState;
        }

        public static void SubscribeToAllTransitions<T>(object subscriber, Action<T, T> onTransition) {
            EventBus.instance.Subscribe(subscriber, TRANSITION_EVENT + (typeof(T)), onTransition);
        }

        public static void SubscribeToTransition<T>(object subscriber, T oldState, T newState, Action onTransition) {
            SubscribeToAllTransitions<T>(subscriber, (old, newS) => {
                if (old.Equals(oldState) && (newState == null || newS.Equals(newState))) { onTransition(); }
            });
        }

    }
}