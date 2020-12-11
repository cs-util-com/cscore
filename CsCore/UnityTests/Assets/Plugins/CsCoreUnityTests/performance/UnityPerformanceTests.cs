using NUnit.Framework;
using System;
using UnityEngine;

namespace com.csutil.tests.performance {

    public class UnityPerformanceTests {

        /// <summary> Shows that new GameObject and GameObject.Instantiate are about the same speed </summary>
        [Test]
        public void TestTimingOfCreatingManyGameObjects() {

            // First check that "new GameObject" scales linearly:
            Action<int> createUsingNewGo = i => { new GameObject("GO Nr " + i); };
            long ms1 = CreateAndMeasureGOCreationSpeed(goCount: 10000, maxMs: 100, createUsingNewGo);
            long ms2 = CreateAndMeasureGOCreationSpeed(goCount: 100000, maxMs: 1000, createUsingNewGo);

            // Now test using Instantiate(templageGo) instead:
            var templateGo = new GameObject();
            Action<int> GoInstantiate = i => { GameObject.Instantiate(templateGo).name = "GO Nr " + i; };
            long ms7 = CreateAndMeasureGOCreationSpeed(goCount: 10000, maxMs: 100, GoInstantiate);
            long ms8 = CreateAndMeasureGOCreationSpeed(goCount: 100000, maxMs: 1000, GoInstantiate);

            // new GameObject() is a little faster then GameObject.Instantiate :
            AssertXTimesFasterAOverB(1.2, ms1, ms7);
            AssertXTimesFasterAOverB(1.2, ms2, ms8);

            // Test how much slower it gets when the GOs are added to a specific parent:
            var parent = new GameObject("Parent");
            Action<int> newGoAndAddToParent = i => { parent.AddChild(new GameObject("GO Nr " + i)); };
            long ms4 = CreateAndMeasureGOCreationSpeed(goCount: 10000, maxMs: 150, newGoAndAddToParent);
            long ms5 = CreateAndMeasureGOCreationSpeed(goCount: 100000, maxMs: 1500, newGoAndAddToParent);

            // Now test using Instantiate(templageGo) with a parent:
            var parent2 = new GameObject("Parent 2");
            Action<int> GoInstantiateWParent = i => { GameObject.Instantiate(templateGo, parent2.transform).name = "GO Nr " + i; };
            long ms10 = CreateAndMeasureGOCreationSpeed(goCount: 10000, maxMs: 100, GoInstantiateWParent);
            long ms11 = CreateAndMeasureGOCreationSpeed(goCount: 100000, maxMs: 1000, GoInstantiateWParent);

            // GameObject.Instantiate(with Parent) is a a little faster then parent.AddChild(new GameObject()) :
            AssertXTimesFasterAOverB(1.2, ms10, ms4);
            AssertXTimesFasterAOverB(1.2, ms11, ms5);

        }

        private void AssertXTimesFasterAOverB(double x, double a, double b) {
            double r = b / a; // Calculate ratio between a and b
            Assert.IsTrue(r >= x, $"Expected a({a}ms) to be {x} times faster than b({b}ms) but was only {r} times faster!");
        }

        private static long CreateAndMeasureGOCreationSpeed(int goCount, int maxMs, Action<int> createAction) {
            var t = Log.MethodEnteredWith("count=" + goCount);
            for (int i = 0; i < goCount; i++) { createAction(i); }
            Log.MethodDone(t);
            var ms = t.ElapsedMilliseconds;
            Assert.True(ms < maxMs, $"Creating {goCount} GameObjects took {ms}ms (which is > {maxMs}ms)");
            return ms;
        }

    }

}