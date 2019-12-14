using com.csutil.ui;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests.ui {

    public class Ui7_Snackbars : MonoBehaviour { IEnumerator Start() { yield return new Ui7_SnackbarTests().ExampleUsage(); } }

    // The automated unit test that is called by the MonoBehaviour or by the Unity Test Runner
    class Ui7_SnackbarTests {
        [UnityTest]
        public IEnumerator ExampleUsage() {
            Snackbar.Show("Snackbar 1", "Click me!", delegate {
                Toast.Show("Snackbar 1 button clicked");
            });
            yield return new WaitForSeconds(1);
            Snackbar.Show("Some snackbar 2");
        }
    }

}