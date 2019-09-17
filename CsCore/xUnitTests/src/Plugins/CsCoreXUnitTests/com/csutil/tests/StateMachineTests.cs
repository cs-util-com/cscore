using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using com.csutil.datastructures;
using com.csutil.encryption;
using com.csutil.random;
using Xunit;

namespace com.csutil.tests {

    public class StateMachineTests {

        public enum MyStates { MyState1, MyState2, MyState3 }

        [Fact]
        public static void StateMachine_ExampleUsage1() {

            // First define a set of allowed transitions to define the state machine:
            var stateMachine = new Dictionary<MyStates, HashSet<MyStates>>();
            stateMachine.AddToValues(MyStates.MyState1, MyStates.MyState2); // 1 => 2 allowed
            stateMachine.AddToValues(MyStates.MyState2, MyStates.MyState3); // 2 => 3 allowed

            // Initialize a state-machine:
            MyStates currentState = MyStates.MyState1;

            // It is possible to listen to state machine transitions:
            StateMachine.SubscribeToAllTransitions<MyStates>(new object(), (machine, oldState, newState) => {
                Log.d("Transitioned from " + oldState + " to " + newState);
            });
            // And its possible to listen only to specific transitions:
            StateMachine.SubscribeToTransition(new object(), MyStates.MyState1, MyStates.MyState2, delegate {
                Log.d("Transitioned from 1 => 2");
            });

            // Transition the state-machine from state 1 to 2:
            currentState = stateMachine.TransitionTo(currentState, MyStates.MyState2);
            Assert.Equal(MyStates.MyState2, currentState);

            // Invalid transitions throw exceptions (current state is 2):
            Assert.Throws<InvalidOperationException>(() => {
                currentState = stateMachine.TransitionTo(currentState, MyStates.MyState1);
            });

        }

        [Fact]
        public static void StateMachine_ExampleUsage2() {

            // Create the state machine similar to the first example but wrapped in a enclosing class (see below):
            var myStateMachine = new MyStateMachineForExample2();

            // The transitions are encapsulared behind methods, so switching state is more enclosed this way:
            myStateMachine.SwitchToSecondState();
            Assert.Equal(MyStates.MyState2, myStateMachine.currentState);

            // Invalid transitions throw exceptions (current state is 2):
            Assert.Throws<InvalidOperationException>(() => { myStateMachine.SwitchToFirstState(); });

        }

        /// <summary> 
        /// This state machine works the same way example usage 1 works only that it's 
        /// encapsulated to prevent direct access to modifying the state
        /// </summary>
        private class MyStateMachineForExample2 {

            private Dictionary<MyStates, HashSet<MyStates>> allowedTransitions;
            public MyStates currentState { get; private set; }

            // In the constructor the allowed transitions and the initial state are set:
            public MyStateMachineForExample2() {
                // First define a set of allowed transitions to define the state machine:
                allowedTransitions = new Dictionary<MyStates, HashSet<MyStates>>();
                allowedTransitions.AddToValues(MyStates.MyState1, MyStates.MyState2);
                allowedTransitions.AddToValues(MyStates.MyState2, MyStates.MyState3);
                // Initialize the state-machine:
                currentState = MyStates.MyState1;
            }

            public void SwitchToFirstState() { SwitchToState(MyStates.MyState1); }
            public void SwitchToSecondState() { SwitchToState(MyStates.MyState2); }
            public void SwitchToFinalState() { SwitchToState(MyStates.MyState3); }
            private void SwitchToState(MyStates newState) {
                currentState = allowedTransitions.TransitionTo(currentState, newState);
            }

        }

        [Fact]
        public static void StateMachine_TransitionEventTests() {

            var stateMachine = new Dictionary<MyStates, HashSet<MyStates>>();
            stateMachine.AddToValues(MyStates.MyState1, MyStates.MyState2);
            MyStates currentState = MyStates.MyState1;

            var listenerForAllTransitionsTriggered = false;
            var listenerForSpecificTransitionTriggered = false;
            var listenerForExitingState1Triggered = false;
            var listenerForEnteringState2Triggered = false;
            StateMachine.SubscribeToAllTransitions<MyStates>(new object(), (passedMachine, oldState, newState) => {
                listenerForAllTransitionsTriggered = true;
                Assert.Equal(stateMachine, passedMachine);
            });
            StateMachine.SubscribeToTransition(new object(), MyStates.MyState1, MyStates.MyState2, (passedMachine) => {
                listenerForSpecificTransitionTriggered = true;
                Assert.Equal(stateMachine, passedMachine);
            });
            StateMachine.SubscribeToStateExited(new object(), MyStates.MyState1, (passedMachine) => {
                listenerForExitingState1Triggered = true;
                Assert.Equal(stateMachine, passedMachine);
            });
            StateMachine.SubscribeToStateEntered(new object(), MyStates.MyState2, (passedMachine) => {
                listenerForEnteringState2Triggered = true;
                Assert.Equal(stateMachine, passedMachine);
            });

            currentState = stateMachine.TransitionTo(currentState, MyStates.MyState2);
            Assert.True(listenerForAllTransitionsTriggered);
            Assert.True(listenerForSpecificTransitionTriggered);
            Assert.True(listenerForExitingState1Triggered);
            Assert.True(listenerForEnteringState2Triggered);

        }

        [Fact]
        public static void StateMachine_TransitionEventTests2() {

            var stateMachine = new Dictionary<MyStates, HashSet<MyStates>>();
            stateMachine.AddToValues(MyStates.MyState1, MyStates.MyState1);
            stateMachine.AddToValues(MyStates.MyState1, MyStates.MyState2);
            stateMachine.AddToValues(MyStates.MyState2, MyStates.MyState2);
            stateMachine.AddToValues(MyStates.MyState2, MyStates.MyState1);
            MyStates currentState = MyStates.MyState1;

            var state1To1Counter = 0;
            StateMachine.SubscribeToTransition<MyStates>(new object(), MyStates.MyState1, MyStates.MyState1, delegate { state1To1Counter++; });
            var state1To2Counter = 0;
            StateMachine.SubscribeToTransition<MyStates>(new object(), MyStates.MyState1, MyStates.MyState2, delegate { state1To2Counter++; });
            var state2To2Counter = 0;
            StateMachine.SubscribeToTransition<MyStates>(new object(), MyStates.MyState2, MyStates.MyState2, delegate { state2To2Counter++; });
            var state2To1Counter = 0;
            StateMachine.SubscribeToTransition<MyStates>(new object(), MyStates.MyState2, MyStates.MyState1, delegate { state2To1Counter++; });
            { // Transition from 1 => 1 => 2 => 2 => 1 :
                currentState = Transition_1_2_2_1(stateMachine, currentState);
                var c = 1; // All listeners should have counted one time:
                Assert.Equal(c, state1To1Counter);
                Assert.Equal(c, state1To2Counter);
                Assert.Equal(c, state2To2Counter);
                Assert.Equal(c, state2To1Counter);
            }
            { // AGAIN Transition from 1 => 1 => 2 => 2 => 1 :
                currentState = Transition_1_2_2_1(stateMachine, currentState);
                var c = 2; // All listeners should have counted AGAIN one time:
                Assert.Equal(c, state1To1Counter);
                Assert.Equal(c, state1To2Counter);
                Assert.Equal(c, state2To2Counter);
                Assert.Equal(c, state2To1Counter);
            }

        }

        private static MyStates Transition_1_2_2_1(Dictionary<MyStates, HashSet<MyStates>> stateMachine, MyStates currentState) {
            currentState = stateMachine.TransitionTo(currentState, MyStates.MyState1);
            currentState = stateMachine.TransitionTo(currentState, MyStates.MyState2);
            currentState = stateMachine.TransitionTo(currentState, MyStates.MyState2);
            currentState = stateMachine.TransitionTo(currentState, MyStates.MyState1);
            return currentState;
        }

    }

}