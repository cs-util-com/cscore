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
                throw new InvalidOperationException(currentState + " -> " + newState + " blocked, " + currentState + " has no allowed transitions");
            } else if (!allowedTransitions[currentState].Contains(newState)) {
                throw new InvalidOperationException(currentState + " -> " + newState + " blocked, transition not allowed!");
            }
            EventBus.instance.Publish(TRANSITION_EVENT + (typeof(T)), allowedTransitions, currentState, newState);
            return newState;
        }

        public static void SubscribeToAllTransitions<T>(object subscriber, Action<Dictionary<T, HashSet<T>>, T, T> onTransition) {
            EventBus.instance.Subscribe(subscriber, TRANSITION_EVENT + (typeof(T)), onTransition);
        }

        public static void SubscribeToTransition<T>(object subscriber, T oldState, T newState, Action<Dictionary<T, HashSet<T>>> onTransition) {
            SubscribeToAllTransitions<T>(subscriber, (sm, old, newS) => {
                if (old.Equals(oldState) && newS.Equals(newState)) { onTransition(sm); }
            });
        }

        public static void SubscribeToStateEntered<T>(object subscriber, T enteredState, Action<Dictionary<T, HashSet<T>>> onTransition) {
            SubscribeToAllTransitions<T>(subscriber, (sm, _, s) => { if (s.Equals(enteredState)) { onTransition(sm); } });
        }

        public static void SubscribeToStateExited<T>(object subscriber, T exitedState, Action<Dictionary<T, HashSet<T>>> onTransition) {
            SubscribeToAllTransitions<T>(subscriber, (sm, s, _) => { if (s.Equals(exitedState)) { onTransition(sm); } });
        }

    }

}