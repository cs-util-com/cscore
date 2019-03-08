using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using com.csutil.datastructures;
using com.csutil.encryption;
using com.csutil.random;
using Xunit;

namespace com.csutil.tests {
    public class StateTests {

        private enum MyStates { MyState1, MyState2, MyState3 }

        [Fact]
        public static void StateMachine_ExampleUsage1() {

            // First define a set of allowed transitions to define the state machine:
            var stateMachine = new Dictionary<MyStates, HashSet<MyStates>>();
            stateMachine.AddToValues(MyStates.MyState1, MyStates.MyState2);
            stateMachine.AddToValues(MyStates.MyState2, MyStates.MyState3);

            // Initialize a state-machine:
            MyStates currentState = MyStates.MyState1;

            // It is possible to listen to state machine transitions:
            StateMachine.SubscribeToAllTransitions(new object(), (MyStates oldState, MyStates newState) => {
                Log.d("Transitioned from " + oldState + " to " + newState);
            });
            // And its possible to listen only to specific transitions:
            StateMachine.SubscribeToTransition(new object(), MyStates.MyState1, MyStates.MyState2, () => {
                Log.d("Transitioned from 1 => 2");
            });

            // Transition the state-machine from state 1 to 2:
            currentState = stateMachine.TransitionTo(currentState, MyStates.MyState2);
            Assert.Equal(MyStates.MyState2, currentState);

            // Invalid transitions throw exceptions (current state is 2):
            Assert.Throws<Exception>(() => {
                currentState = stateMachine.TransitionTo(currentState, MyStates.MyState1);
            });

        }

        [Fact]
        public static void StateMachine_ExampleUsage2() {

            // It is possible to listen to state machine transitions:
            StateMachine.SubscribeToAllTransitions(new object(), (MyStates oldState, MyStates newState) => {
                Log.d("Transitioned from " + oldState + " to " + newState);
            });
            // And its possible to listen only to specific transitions:
            StateMachine.SubscribeToTransition(new object(), MyStates.MyState1, MyStates.MyState2, () => {
                Log.d("Transitioned from 1 => 2");
            });

            // Create the state machine:
            var stateMachine = new MyExampleStateMachine();

            stateMachine.SwitchToSecondState();
            Assert.Equal(MyStates.MyState2, stateMachine.currentState);

            // Invalid transitions throw exceptions (current state is 2):
            Assert.Throws<Exception>(() => { stateMachine.SwitchToFirstState(); });

        }

        private class MyExampleStateMachine {
            private Dictionary<MyStates, HashSet<MyStates>> stateMachine;
            public MyStates currentState { get; private set; }

            public MyExampleStateMachine() {
                // First define a set of allowed transitions to define the state machine:
                stateMachine = new Dictionary<MyStates, HashSet<MyStates>>();
                stateMachine.AddToValues(MyStates.MyState1, MyStates.MyState2);
                stateMachine.AddToValues(MyStates.MyState2, MyStates.MyState3);

                // Initialize a state-machine:
                currentState = MyStates.MyState1;
            }

            internal void SwitchToSecondState() {
                currentState = stateMachine.TransitionTo(currentState, MyStates.MyState2);
            }

            internal void SwitchToFirstState() {
                currentState = stateMachine.TransitionTo(currentState, MyStates.MyState1);
            }

        }

    }

}