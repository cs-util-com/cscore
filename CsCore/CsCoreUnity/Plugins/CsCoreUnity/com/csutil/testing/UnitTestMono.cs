﻿using com.csutil.ui;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    /// <summary>
    /// This class can be instantiated by any normal runtime unit test and then its RunTest() method can be called, e.g.:
    /// <para/>  public class MyUiScreen1Tests {
    /// <para/>     [UnityTest] 
    /// <para/>     public IEnumerator MyTestCase1() { 
    /// <para/>         yield return UnitTestMono.RunTest<MyUnitTestMono1>(parent: new GameObject()); 
    /// <para/>     }
    /// <para/>  } 
    /// </summary>
    public abstract class UnitTestMono : MonoBehaviour {

        /// <summary> Will be true when the mono is executed in a unit test </summary>
        public bool simulateUserInput;
        private bool isTestStarted;

        public abstract IEnumerator RunTest();

        public virtual IEnumerator Start() {
            yield return new WaitForSeconds(0.010f); // Wait 10ms before starting the test
            if (!isTestStarted) {
                isTestStarted = true;
                yield return RunTest();
            }
            yield return null;
        }

        public static IEnumerator RunTest<T>(string prefabName) where T : UnitTestMono {
            ViewStack viewStack = ViewStackHelper.MainViewStack();
            yield return RunTest<T>(viewStack, prefabName);
        }

        public static IEnumerator RunTest<T>(ViewStack viewStack, string prefabName) where T : UnitTestMono {
            var ui = viewStack.ShowView(prefabName);
            yield return RunTest<T>(ui);
        }

        public static IEnumerator RunTest<T>(GameObject parent) where T : UnitTestMono {
            AssertV2.throwExeptionIfAssertionFails = true;
            AssertV3.throwExeptionIfAssertionFails = true;
            var testMono = parent.GetOrAddComponent<T>();
            testMono.simulateUserInput = true;
            yield return testMono.Start();
        }

        /// <summary>
        /// Will search for a button with the passed Link-id and trigger it if simulateUserInput is true
        /// </summary>
        public void SimulateButtonClickOn(string buttonName) {
            if (simulateUserInput) {
                Log.d("Now simulating the user clicking the button=" + buttonName);
                GetLink(buttonName).GetComponentV2<Button>().onClick.Invoke();
            }
        }

        public Task SimulateButtonClickOn(string buttonName, IList<Task> buttonClicks) {
            return SimulateButtonClickOn(buttonName, buttonClicks.Last());
        }

        public Task SimulateButtonClickOn(string buttonName, Task buttonClickWaitTask) {
            AssertV3.IsFalse(buttonClickWaitTask.IsCompleted, () => "buttonClickWaitTask was already completed");
            SimulateButtonClickOn(buttonName);
            return buttonClickWaitTask;
        }

        public static IEnumerable<Link> FindAllActiveLinks() {
            return ResourcesV2.FindAllInScene<Link>().Filter(x => x.isActiveAndEnabled);
        }

        public static Link GetLink(string name) { return FindAllActiveLinks().Single(x => x.id == name); }

    }

}