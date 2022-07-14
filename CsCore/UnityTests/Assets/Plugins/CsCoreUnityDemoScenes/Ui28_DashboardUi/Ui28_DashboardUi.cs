using System.Collections;
using System.Threading.Tasks;
using com.csutil.ui;

namespace com.csutil.tests.ui {

    public class Ui28_DashboardUi : UnitTestMono {

        public override IEnumerator RunTest() { yield return RunTestTask().AsCoroutine(); }

        private async Task RunTestTask() {
            GetComponent<TabsUiManager>();
        }

    }

}