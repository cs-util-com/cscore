using System;
using System.Collections;

namespace com.csutil.tests.ui19 {

    public class Ui19_VisualRegressionTesting : UnitTestMono {

        public override IEnumerator RunTest() {

            var folderToStoreImagesIn = EnvironmentV2.instance.GetCurrentDirectory().GetChildDir("VisualRegressionTesting");
            var i = new PersistedImageRegression(folderToStoreImagesIn);

            i.AssertEqualToPersisted("img1");

            yield return null;

        }

    }

}