using System;
using System.Linq;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    public class DataStoreStressTests {

        public DataStoreStressTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void StressTest1() {

            var initialState = new MyAppState1(new SubStateA("a1"), new SubStateB("b1"));
            var store = new DataStore<MyAppState1>(ReduceMyAppState1, initialState);

            // In total this test will create 4 million state change listeners:
            int listenerCount = 1000000;
            StopwatchV2 t1, t2, t3, t4; // The 4 measured timings of the Dispatches
            { // Add subListeners that are only informed by the one listener attached directly to the store:
                var counterA1 = 0;
                var subListenersA = store.NewSubStateListener(s => s.substateA);
                for (int i = 0; i < listenerCount; i++) {
                    subListenersA.AddStateChangeListener(substateA => substateA.valA, newValA => {
                        counterA1++;
                    });
                }
                t1 = Log.MethodEntered("ActionChangeSubstateA");
                store.Dispatch(new ActionChangeSubstateA() { newVal = "a2" });
                Log.MethodDone(t1);
                Assert.Equal("a2", store.GetState().substateA.valA);
                Assert.Equal(listenerCount, counterA1);
            }
            { // Now add additional listeners to check if it makes Dispatching slower:
                var counterB1 = 0;
                var subListenersB = store.NewSubStateListener(s => s.substateB);
                for (int i = 0; i < listenerCount; i++) {
                    subListenersB.AddStateChangeListener(substateB => substateB.valB, newValB => {
                        counterB1++;
                    });
                }
                t2 = Log.MethodEntered("ActionChangeSubstateB");
                store.Dispatch(new ActionChangeSubstateB() { newVal = "b2" });
                Log.MethodDone(t2);
                Assert.Equal("b2", store.GetState().substateB.valB);
                Assert.Equal(listenerCount, counterB1);

                // Make sure the additional listeners did not make the Dispatch slower:
                float t1t2Ratio = (float)t1.ElapsedMilliseconds / (float)t2.ElapsedMilliseconds;
                Assert.True(0.5f < t1t2Ratio && t1t2Ratio < 1.5f, "t1t2Ratio=" + t1t2Ratio);
            }
            { // Now add the listeners directly to the store which slows down the Dispatches:
                var counterA2 = 0;
                var counterB2 = 0;
                for (int i = 0; i < listenerCount; i++) {
                    store.AddStateChangeListener(s => s.substateA, newSubA => { counterA2++; });
                    store.AddStateChangeListener(s => s.substateB, newSubB => { counterB2++; });
                }

                t3 = Log.MethodEntered("ActionChangeSubstateA2");
                store.Dispatch(new ActionChangeSubstateA() { newVal = "a3" });
                Log.MethodDone(t3);
                Assert.Equal("a3", store.GetState().substateA.valA);
                Assert.Equal(listenerCount, counterA2);

                t4 = Log.MethodEntered("ActionChangeSubstateB2");
                store.Dispatch(new ActionChangeSubstateB() { newVal = "b3" });
                Log.MethodDone(t4);
                Assert.Equal("b3", store.GetState().substateB.valB);
                Assert.Equal(listenerCount, counterB2);

                // Make sure the additional listeners make Dispatching much slower:
                float t1t3Ratio = (float)t3.ElapsedMilliseconds / (float)t1.ElapsedMilliseconds;
                Log.d("t1t3Ratio=" + t1t3Ratio);
                Assert.True(2.5f < t1t3Ratio, "t1t3Ratio=" + t1t3Ratio);

                float t1t4Ratio = (float)t4.ElapsedMilliseconds / (float)t1.ElapsedMilliseconds;
                Log.d("t1t4Ratio=" + t1t4Ratio);
                Assert.True(2.5f < t1t4Ratio, "t1t4Ratio=" + t1t4Ratio);

                float t3t4Ratio = (float)t3.ElapsedMilliseconds / (float)t4.ElapsedMilliseconds;
                Log.d("t3t4Ratio=" + t3t4Ratio);
                Assert.True(0.9f < t3t4Ratio && t3t4Ratio < 1.1f, "t3t4Ratio=" + t3t4Ratio);

            }
        }

        private MyAppState1 ReduceMyAppState1(MyAppState1 previousState, object action) {
            bool changed = false;
            var newA = previousState.substateA.Mutate(action, SubStateAReducer, ref changed);
            var newB = previousState.substateB.Mutate(action, SubStateBReducer, ref changed);
            if (changed) { return new MyAppState1(newA, newB); }
            return previousState;
        }

        private SubStateA SubStateAReducer(SubStateA previousState, object action) {
            if (action is ActionChangeSubstateA a) { return new SubStateA(a.newVal); }
            return previousState;
        }

        private SubStateB SubStateBReducer(SubStateB previousState, object action) {
            if (action is ActionChangeSubstateB b) { return new SubStateB(b.newVal); }
            return previousState;
        }

        private class MyAppState1 {
            internal readonly SubStateA substateA;
            internal readonly SubStateB substateB;
            public MyAppState1(SubStateA newA, SubStateB newB) { substateA = newA; substateB = newB; }
        }

        internal class SubStateA {
            internal readonly string valA;
            public SubStateA(string newVal) { valA = newVal; }
        }

        internal class SubStateB {
            internal readonly string valB;
            public SubStateB(string newVal) { valB = newVal; }
        }

        private class ActionChangeSubstateA { internal string newVal; }

        private class ActionChangeSubstateB { internal string newVal; }

    }

}
