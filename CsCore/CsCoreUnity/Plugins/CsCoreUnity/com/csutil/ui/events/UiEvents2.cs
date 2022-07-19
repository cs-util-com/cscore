using System.Threading.Tasks;
using UnityEngine.UI;

namespace com.csutil.ui {

    public static class UiEvents {

        public const string INPUTFIELD_CHANGED = "InputFieldChanged";
        public const string TOGGLE_CHANGED = "ToggleChanged";
        public const string BUTTON_CLICKED = "ButtonClick";
        public const string SLIDER_CHANGED = "SliderChanged";
        public const string DROPDOWN_CHANGED = "DropDownChanged";

        public const string ACTION_MENU = "ActionMenu";
        public const string DIALOG = "Dialog";
        public const string CONFIRM_CANCEL_DIALOG = "ConfirmDialog";

        public static Task<bool> WaitForToggleToBeChecked(string targetId) { return WaitForToggleToBeChecked(targetId, new object()); }

        public static Task<bool> WaitForToggleToBeChecked(string targetId, object subscriber) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            var eventName = EventConsts.catUi + TOGGLE_CHANGED;
            EventBus.instance.Subscribe(subscriber, eventName, (Toggle toggle, bool isChecked) => {
                var isCorrectToggle = toggle.GetComponent<Link>().id == targetId;
                if (isCorrectToggle && isChecked) {
                    var unsubscribed = EventBus.instance.Unsubscribe(subscriber, eventName);
                    AssertV2.IsTrue(unsubscribed, "unsubscribed event listener");
                    tcs.SetResult(true);
                }
            });
            var waitForToggleToBeChecked = tcs.Task;
            return waitForToggleToBeChecked;
        }

    }

}