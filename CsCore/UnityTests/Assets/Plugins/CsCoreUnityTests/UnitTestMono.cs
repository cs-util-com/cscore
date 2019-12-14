using com.csutil.ui;
using System.Collections;
using UnityEngine;

namespace com.csutil.tests {

    public abstract class UnitTestMono : MonoBehaviour {

        /// <summary> Creates a new view stack and adds the specified Prefab to this stack </summary>
        /// <returns> The IEnumerator that can be awaited by the calling test </returns>
        public static IEnumerator LoadAndRunUiTest(string prefabName) {
            var viewStack = CanvasFinder.GetOrAddRootCanvas().gameObject.AddComponent<ViewStack>();
            yield return viewStack.ShowView(prefabName).FindAndRunTest();
        }

        /// <summary>  Has to be set to true by the unit test that wants to call RunTest manually  </summary>
        public bool callRunTestManually { get; set; }

        public virtual IEnumerator Start() {
            if (!callRunTestManually) { yield return RunTest(); }
            yield return null;
        }

        public abstract IEnumerator RunTest();

    }

    public static class TestMonoExtensions {

        public static IEnumerator FindAndRunTest(this GameObject self) {
            var testMono = self.GetComponentInChildren<UnitTestMono>();
            testMono.callRunTestManually = true;
            yield return testMono.RunTest();
        }

    }

}
