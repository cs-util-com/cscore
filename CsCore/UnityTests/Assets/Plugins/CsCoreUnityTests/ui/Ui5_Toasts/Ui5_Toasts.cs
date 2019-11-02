using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests.ui {

    // This MonoBehaviour can be used for manual tests by attaching it to a scene
    public class Ui5_Toasts : MonoBehaviour { IEnumerator Start() { yield return new Ui5_ToastsTests().ExampleUsage(); } }

    // The automated unit test that is called by the MonoBehaviour or by the Unity Test Runner
    class Ui5_ToastsTests {
        [UnityTest]
        public IEnumerator ExampleUsage() {
            Toast.Show("Some toast 1", "Lorem ipsum 1");
            yield return new WaitForSeconds(1);
            Toast.Show("Some toast 2");
            yield return new WaitForSeconds(1);
            Toast.Show("Some toast 3", "Lorem ipsum 3", 1000);
            yield return new WaitForSeconds(2);
        }
    }

}