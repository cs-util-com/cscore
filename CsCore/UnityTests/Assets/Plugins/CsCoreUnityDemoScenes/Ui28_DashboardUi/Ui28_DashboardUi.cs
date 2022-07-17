using System.Collections;
using System.Threading.Tasks;
using com.csutil.ui;

namespace com.csutil.tests.ui {

    public class Ui28_DashboardUi : UnitTestMono {

        public override IEnumerator RunTest() { yield return RunTestTask().AsCoroutine(); }

        private async Task RunTestTask() {
            var ui = gameObject.GetLinkMap();

            var sidebar = ui.Get<TabsUiManager>("Sidebar");
            sidebar.onTabRequested = (linkId) => {
                switch (linkId) {
                    case "Show Panel 1": return "Ui28_Panel1";
                    case "Show Panel 2": return "Ui28_Panel2";
                    default: return null;
                }
            };
            sidebar.onCustomTabClickAction = (linkId, tabsPanel) => {
                if (linkId == "Close Application") {
                    CloseApp();
                } else {
                    Log.e("Unknown linkId: " + linkId);
                }
            };
        }

        private static async Task CloseApp() {
            var userConfirmed = await ConfirmCancelDialog.Show("Quit", "Do you want to exit the application?");
            if (userConfirmed) { ApplicationV2.Quit(); }
        }

    }

}