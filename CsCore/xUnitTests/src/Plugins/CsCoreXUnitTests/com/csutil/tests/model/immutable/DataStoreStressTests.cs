using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.integrationTests.model.immutable {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class DataStoreStressTests {

        public DataStoreStressTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void StressTest1() {

            var initialState = new MyAppState1(new SubStateA("a1"), new SubStateB("b1"), NewVeryLargeList());
            var store = new DataStore<MyAppState1>(ReduceMyAppState1, initialState);

            // In total this test will create 4 million state change listeners:
            int listenerCount = 100000;
            StopwatchV2 t1, t2, t3, t4; // The 4 measured timings of the Dispatches
            { // Add subListeners that are only informed by the one listener attached directly to the store:
                var counterA1 = 0;
                var subListenersA = store.GetSubState(s => s.substateA);
                for (int i = 0; i < listenerCount; i++) {
                    subListenersA.AddStateChangeListener(substateA => substateA.valA, newValA => {
                        counterA1++;
                    }, triggerInstantToInit: false);
                }
                t1 = Log.MethodEntered("ActionChangeSubstateA");
                store.Dispatch(new ActionChangeSubstateA() { newVal = "a2" });
                Log.MethodDone(t1);
                Assert.Equal("a2", store.GetState().substateA.valA);
                Assert.Equal(listenerCount, counterA1);
            }
            { // Now add additional listeners to check if it makes Dispatching slower:
                var counterB1 = 0;
                var subListenersB = store.GetSubState(s => s.substateB);
                for (int i = 0; i < listenerCount; i++) {
                    subListenersB.AddStateChangeListener(substateB => substateB.valB, newValB => {
                        counterB1++;
                    }, triggerInstantToInit: false);
                }
                t2 = Log.MethodEntered("ActionChangeSubstateB");
                store.Dispatch(new ActionChangeSubstateB() { newVal = "b2" });
                Log.MethodDone(t2);
                Assert.Equal("b2", store.GetState().substateB.valB);
                Assert.Equal(listenerCount, counterB1);

                // Make sure the additional listeners did not make the Dispatch slower:
                float t1t2Ratio = (float)t1.ElapsedMilliseconds / (float)t2.ElapsedMilliseconds;
                Log.d("t1t2Ratio=" + t1t2Ratio);
                Assert.True(0.3f < t1t2Ratio && t1t2Ratio < 3f, "t1t2Ratio=" + t1t2Ratio);
            }
            { // Now add the listeners directly to the store which slows down the Dispatches:
                var counterA2 = 0;
                var counterB2 = 0;
                for (int i = 0; i < listenerCount; i++) {
                    store.AddStateChangeListener(s => s.substateA, newSubA => {
                        counterA2++;
                    }, triggerInstantToInit: false);
                    store.AddStateChangeListener(s => s.substateB, newSubB => {
                        counterB2++;
                    }, triggerInstantToInit: false);
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
                Assert.True(1.5f < t1t3Ratio, "t1t3Ratio=" + t1t3Ratio);

                float t1t4Ratio = (float)t4.ElapsedMilliseconds / (float)t1.ElapsedMilliseconds;
                Log.d("t1t4Ratio=" + t1t4Ratio);
                Assert.True(1.5f < t1t4Ratio, "t1t4Ratio=" + t1t4Ratio);

                float t3t4Ratio = (float)t3.ElapsedMilliseconds / (float)t4.ElapsedMilliseconds;
                Log.d("t3t4Ratio=" + t3t4Ratio);
                Assert.True(0.2f < t3t4Ratio && t3t4Ratio < 2f, "t3t4Ratio=" + t3t4Ratio);

            }

            TestListEntrySelector(store);

        }

        /// <summary>
        /// if the data model contains lists traversing through the model tree into the list to get a 
        /// specific element can be a costly operation if performed many times. Normally indexed 
        /// structures like dictionaries should be used to efficiently access such collections but 
        /// when a list in the model is needed the index of the element can be used to make accessing
        /// the entry more efficient. The following test demonstrates this efficiency difference
        /// </summary>
        private static void TestListEntrySelector(DataStore<MyAppState1> store) {

            var lastEntry = store.GetState().substateC.listC.Last();

            // First the usual way is measured where the normal Find is used to get the wanted list entry:
            var findUsedCount = 400;
            var timingUsingFind = Log.MethodEntered($"Find in list ({findUsedCount} times)");
            for (int i = 0; i < findUsedCount; i++) {
                var foundEntry = store.GetState().substateC.listC.Find(x => x.valB == lastEntry.valB);
                Assert.Equal(lastEntry, foundEntry);
            }
            Log.MethodDone(timingUsingFind);

            // Now the optimized version to find the list entry is shown:
            Func<SubStateB> entrySelector = store.SelectListEntry(x => x.substateC.listC, e => e.valB == lastEntry.valB);

            var selectorTiming1 = Log.MethodEntered($"Find selector 1 ({findUsedCount} times)");
            for (int i = 0; i < findUsedCount / 2; i++) {
                var foundEntry = entrySelector();
                Assert.Equal(lastEntry, foundEntry);
            }
            Log.MethodDone(selectorTiming1);

            // Remove the first entry from the immutable list so that the index of the last entry changes:
            var oldList = store.GetState().substateC.listC;
            var newList = oldList.Remove(oldList.First());
            store.Dispatch(new ActionChangeSubstateC() { newVal = newList });

            // Check that the selector still finds the last entry after modification now:
            var selectorTiming2 = Log.MethodEntered($"Find selector 2 ({findUsedCount} times)");
            for (int i = 0; i < findUsedCount / 2; i++) {
                var foundEntry = entrySelector();
                Assert.Equal(lastEntry, foundEntry);
            }
            Log.MethodDone(selectorTiming2);

            // The optimized selector version must be at least 100 times faster then using normal find:
            long xTimesFaster = 100;
            var totalSelectorTime = selectorTiming1.ElapsedMilliseconds + selectorTiming2.ElapsedMilliseconds;
            var errorT = $"timingUsingSelector={totalSelectorTime} not {xTimesFaster} times faster then " +
                $"timingUsingFind={timingUsingFind.ElapsedMilliseconds}";
            Assert.True(totalSelectorTime < timingUsingFind.ElapsedMilliseconds / xTimesFaster, errorT);

        }

        private SubStateC NewVeryLargeList(int entries = 100000) {
            var largeList = new List<SubStateB>();
            for (int i = 0; i < entries; i++) { largeList.Add(new SubStateB("Entry " + i)); }
            return new SubStateC(largeList.ToImmutableList());
        }

        private MyAppState1 ReduceMyAppState1(MyAppState1 previousState, object action) {
            bool changed = false;
            var newA = previousState.substateA.Mutate(action, SubStateAReducer, ref changed);
            var newB = previousState.substateB.Mutate(action, SubStateBReducer, ref changed);
            var newC = previousState.substateC.Mutate(action, SubStateCReducer, ref changed);
            if (changed) { return new MyAppState1(newA, newB, newC); }
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

        private SubStateC SubStateCReducer(SubStateC previousState, object action) {
            if (action is ActionChangeSubstateC c) { return new SubStateC(c.newVal); }
            return previousState;
        }

        private class MyAppState1 {
            internal readonly SubStateA substateA;
            internal readonly SubStateB substateB;
            internal readonly SubStateC substateC;
            public MyAppState1(SubStateA newA, SubStateB newB, SubStateC newC) {
                substateA = newA;
                substateB = newB;
                substateC = newC;
            }
        }

        internal class SubStateA {
            internal readonly string valA;
            public SubStateA(string newVal) { valA = newVal; }
        }

        internal class SubStateB {
            internal readonly string valB;
            public SubStateB(string newVal) { valB = newVal; }
        }

        internal class SubStateC {
            internal readonly ImmutableList<SubStateB> listC;
            public SubStateC(ImmutableList<SubStateB> newVal) { listC = newVal; }
        }

        private class ActionChangeSubstateA { internal string newVal; }

        private class ActionChangeSubstateB { internal string newVal; }

        internal class ActionChangeSubstateC { internal ImmutableList<SubStateB> newVal; }

    }

}
