using com.csutil.ui.jsonschema;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.tests.ui19 {

    public class Ui19_VisualRegressionTesting : UnitTestMono {

        public override IEnumerator RunTest() {
            yield return ExampleUsage1().AsCoroutine();
            yield return RunVisualRegressionErrorExample().AsCoroutine();
        }

        private async Task ExampleUsage1() {

            // First create a default instance and pass it as a singleton to the injection logic:
            AssertVisually.SetupDefaultSingletonInDebugMode();

            // Create and show a UI based on the fields in MyUserModelv1:
            gameObject.AddChild(await NewUiFor<MyUserModelv1>());
            await AssertVisually.AssertNoVisualChange("Ui for MyUserModelv1");

        }



        private async Task RunVisualRegressionErrorExample() {

            // Create an AssertVisually instance for testing:
            AssertVisually assertVisually = NewAssertVisuallyInstance("Ui19_RunVisualRegressionErrorExample");

            // Create and show a UI based on the fields in MyUserModelv1:
            gameObject.AddChild(await NewUiFor<MyUserModelv1>());
            await assertVisually.AssertNoVisualChange("UserUiScreen");

            Toast.Show("Will now load a slightly different UI to cause a visual regression error..");
            await TaskV2.Delay(4000);

            // Now load MyUserModelv2 which creates a slightly different UI compared to MyUserModelv1:
            gameObject.AddChild(await NewUiFor<MyUserModelv2>());
            await assertVisually.AssertNoVisualChange("UserUiScreen");
            Toast.Show("Now a visual assertion error should show in the console");

        }

        private static AssertVisually NewAssertVisuallyInstance(string folderName) {
            var folderToStoreImagesIn = EnvironmentV2.instance.GetCurrentDirectory().GetChildDir(folderName);
            folderToStoreImagesIn.DeleteV2();
            return new AssertVisually(folderToStoreImagesIn);
        }

        /// <summary> Generate a UI from the passed arbitrary class </summary>
        private static async Task<GameObject> NewUiFor<T>() {
            return await JsonSchemaToView.NewViewGenerator().GenerateViewFrom<T>();
        }

#pragma warning disable 0649 // Variable is never assigned to, and will always have its default value
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
#pragma warning restore 0649 // Variable is never assigned to, and will always have its default value

    }

}