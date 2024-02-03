using System.Collections;
using System.Linq;
using com.csutil.ui;
using UnityEngine;

namespace com.csutil.tests.ui {

    public class Ui5_Toasts : UnitTestMono {

        public string someUiScreenPrefabName = "Canvas/DefaultViewStackView";

        public override IEnumerator RunTest() {

            // Show some empty view as a background for the toasts:
            ViewStackHelper.MainViewStack().ShowView(someUiScreenPrefabName);

            Toast.Show("Some toast 1", "Lorem ipsum 1");
            yield return new WaitForSeconds(1);
            Toast.Show("Some toast 2");

            // In between show another screen on the main view stack, to ensure it does not interrupt showing the toasts:
            ViewStackHelper.MainViewStack().SwitchToView(someUiScreenPrefabName);

            yield return new WaitForSeconds(1);
            Toast.Show("Some toast 3", "Lorem ipsum 3", 2500);
            yield return new WaitForSeconds(3);

            RootCanvas.GetAllRootCanvases().Single().gameObject.Destroy();
            Toast.Show("Some toast 4");
            yield return new WaitForSeconds(3);

        }

    }

}