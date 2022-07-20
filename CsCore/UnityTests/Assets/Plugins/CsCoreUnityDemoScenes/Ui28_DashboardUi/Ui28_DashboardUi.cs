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
                    CloseApp().LogOnError();
                } else {
                    Log.e("Unknown linkId: " + linkId);
                }
            };

            ShowTutorial();
        }

        private static async Task CloseApp() {
            var userConfirmed = await ConfirmCancelDialog.Show("Quit", "Do you want to exit the application?");
            if (userConfirmed) { ApplicationV2.Quit(); }
        }

        private static async Task ShowTutorial() {
            {
                var instructions = Snackbar.Show("Click on the 'Users' icon to switch to tab 2", -1);
                await UiEvents.WaitForToggle("Show Panel 2", true);
                instructions.Destroy();
            }
            {
                var instructions = Snackbar.Show("Now press the shown button", -1);
                await UiEvents.WaitForButtonToBePressed("Panel2_Button1");
                instructions.Destroy();
            }
            {
                var instructions = Snackbar.Show("Now enter 'abc' in the input field", -1);
                await UiEvents.WaitForInputField("Panel2_InputField1", input => "abc" == input);
                instructions.Destroy();
            }
            {
                var instructions = Snackbar.Show("Now select Option B from the dropdown", -1);
                await UiEvents.WaitForDropDown("Panel2_DropDown1", dropDownEntry => 1 == dropDownEntry);
                instructions.Destroy();
            }
            {
                var instructions = Snackbar.Show("Now switch back to tab 1", -1);
                await UiEvents.WaitForToggle("Show Panel 1", true);
                instructions.Destroy();
            }
        }

    }

}