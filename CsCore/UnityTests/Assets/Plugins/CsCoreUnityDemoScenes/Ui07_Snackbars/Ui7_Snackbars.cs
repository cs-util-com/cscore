using System.Collections;
using UnityEngine;

namespace com.csutil.tests.ui {

    public class Ui7_Snackbars : UnitTestMono {

        public override IEnumerator RunTest() {
            Snackbar.Show("Snackbar 1", "Click me!", delegate {
                Toast.Show("Snackbar 1 button clicked");
            });
            yield return new WaitForSeconds(1);
            Snackbar.Show("Some snackbar 2");
        }

    }

}