using com.csutil.ui.jsonschema;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.tests.ui19 {

    public class Ui19_VisualRegressionTesting : UnitTestMono {

        public override IEnumerator RunTest() {

            yield return ShowSomeUi<MyUserModelv1>().AsCoroutine();

            var folderToStoreImagesIn = EnvironmentV2.instance.GetCurrentDirectory().GetChildDir("VisualRegressionTesting");
            var visualRegressionTester = new PersistedImageRegression(folderToStoreImagesIn);
            yield return visualRegressionTester.AssertEqualToPersisted("UserUiScreen");

            Toast.Show("Will now load a slightly different UI to cause a visual regression error..");
            yield return new WaitForSeconds(4);

            // Now load MyUserModelv2 which creates a slightly different UI compared to MyUserModelv1:
            yield return ShowSomeUi<MyUserModelv2>().AsCoroutine();
            yield return visualRegressionTester.AssertEqualToPersisted("UserUiScreen");

        }

        private async Task ShowSomeUi<T>() { // Generate a UI from the passed class:
            gameObject.AddChild(await JsonSchemaToView.NewViewGenerator().GenerateViewFrom<T>());
        }

        private class MyUserModelv1 {
            public string name;
            public string email;
            public int age;
        }

        private class MyUserModelv2 {
            public string firstAndLastName;
            public string email;
            public int age;
        }

    }

}