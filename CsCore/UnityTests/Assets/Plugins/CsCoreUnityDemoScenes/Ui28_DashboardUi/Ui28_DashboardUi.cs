using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.ui;

namespace com.csutil.tests.ui {

    public class Ui28_DashboardUi : UnitTestMono {

        public override IEnumerator RunTest() { yield return RunTestTask().AsCoroutine(); }

        private async Task RunTestTask() {
            var ui = gameObject.GetLinkMap();
            ui.Get<TabsUiManager>("Sidebar").Setup((panel, activeToggles) => {
                var activeToggle = activeToggles.Single();
                switch (activeToggle.GetComponent<Link>().id) {
                    case "Show Panel 1":
                        panel.SwitchToView("Ui28_Panel1");
                        break;
                    case "Show Panel 2":
                        panel.SwitchToView("Ui28_Panel2");
                        break;
                    default:
                        Log.e("No action defined for " + activeToggle.GetComponent<Link>().id, activeToggle);
                        break;
                }
            });
        }

    }

}