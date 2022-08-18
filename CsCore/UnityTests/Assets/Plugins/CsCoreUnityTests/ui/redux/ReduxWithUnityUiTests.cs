using com.csutil.tests;
using com.csutil.tests.ui;
using System.Collections;
using UnityEngine.TestTools;

namespace CsCoreUnityTests.ui.redux {

    public class ReduxWithUnityUiTests {

        [UnityTest]
        public IEnumerator Ui26_MoreReduxWithUnityUiExamples() {
            yield return UnitTestMono.RunTest<Ui26_MoreReduxWithUnityUiExamples>("Ui26_MoreReduxWithUnityUiExamples");
        }

    }

}