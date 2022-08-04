using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests.ui {

    class UiDemoSceneTests {

        [UnityTest]
        public IEnumerator Ui1_Presenters() {
            yield return UnitTestMono.RunTest<Ui1_Presenters>("MyUserUi1");
        }

        [UnityTest]
        public IEnumerator Ui2_Screens() {
            yield return UnitTestMono.RunTest<Ui2_Screens>("ExampleUi2_MainScreen");
        }

        [UnityTest]
        public IEnumerator Ui3_DataStore() {
            yield return UnitTestMono.RunTest<Ui3_DataStore>(new GameObject());
        }

        [UnityTest]
        public IEnumerator Ui5_Toasts() {
            yield return UnitTestMono.RunTest<Ui5_Toasts>(new GameObject());
        }

        [UnityTest]
        public IEnumerator Ui7_Snackbars() {
            yield return UnitTestMono.RunTest<Ui7_Snackbars>(new GameObject());
        }

        //[UnityTest]
        //public IEnumerator Ui8_LogConsole() {
        //    yield return UnitTestMono.RunTest<Ui8_LogConsole>(new GameObject());
        //}

        [UnityTest]
        public IEnumerator Ui9_AwaitDialog() {
            yield return UnitTestMono.RunTest<Ui9_AwaitDialog>(new GameObject());
        }

        [UnityTest]
        public IEnumerator Ui10_AppFlowTracking() {
            yield return UnitTestMono.RunTest<Ui10_AppFlowTracking>("Ui10_Screen1");
        }

        [UnityTest]
        public IEnumerator Ui12_ActionMenu() {
            yield return UnitTestMono.RunTest<Ui12_ActionMenu>("Ui12_ActionMenu");
        }

        [UnityTest]
        public IEnumerator Ui13_Localization() {
            yield return UnitTestMono.RunTest<Ui13_Localization>("Ui13_Localization");
        }

        [UnityTest]
        public IEnumerator Ui14_ImageLoading() {
            yield return UnitTestMono.RunTest<Ui14_ImageLoading>("Ui14_ImageLoading");
        }
        
        [UnityTest]
        public IEnumerator Ui29_KeyValueStoreMonitor() {
            yield return UnitTestMono.RunTest<Ui29_KeyValueStoreMonitor>("Ui29_KeyValueStoreMonitor");
        }

    }

}
