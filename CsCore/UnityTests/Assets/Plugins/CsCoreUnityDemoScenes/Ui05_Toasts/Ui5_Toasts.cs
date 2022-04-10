using System.Collections;
using com.csutil.ui;
using UnityEngine;

namespace com.csutil.tests.ui {

    public class Ui5_Toasts : UnitTestMono {

        public override IEnumerator RunTest() {

            // Show some empty view as a background for the toasts:
            ViewStackHelper.GetOrAddMainViewStack().ShowView("Canvas/DefaultViewStackView");
            
            Toast.Show("Some toast 1", "Lorem ipsum 1");
            yield return new WaitForSeconds(1);
            Toast.Show("Some toast 2");
            yield return new WaitForSeconds(1);
            var toast3 = Toast.Show("Some toast 3", "Lorem ipsum 3", 1000);
            AssertV2.IsFalse(toast3.IsDestroyed(), "Toast was already destroyed");
            yield return new WaitForSeconds(2);
            AssertV2.IsTrue(toast3.IsDestroyed(), "Toast could not be destroyed");
        }

    }

}