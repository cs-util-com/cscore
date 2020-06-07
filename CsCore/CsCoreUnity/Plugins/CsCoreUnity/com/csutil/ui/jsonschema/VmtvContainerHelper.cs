using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public static class VmtvContainerUtil {

        public static async Task ChangesSavedViaConfirmButton(GameObject targetView, string confirmButtonId = "ConfirmButton") {
            do {
                await ConfirmButtonClicked(targetView, confirmButtonId);
            } while (!RegexValidator.IsAllInputCurrentlyValid(targetView));
        }

        private static Task ConfirmButtonClicked(GameObject targetView, string confirmButtonId) {
            return targetView.GetLinkMap().Get<Button>(confirmButtonId).SetOnClickAction(async delegate {
                Toast.Show("Saving..");
                await TaskV2.Delay(500); // Wait for potential pending throttled actions to update the model
            });
        }

    }

}
