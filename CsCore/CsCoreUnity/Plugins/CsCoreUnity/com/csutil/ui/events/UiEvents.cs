using System;
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

        public static Task<Button> WaitForButtonToBePressed(string targetId) {
            return WaitForButtonToBePressed(targetId, new object());
        }

        public static Task<Button> WaitForButtonToBePressed(string targetId, object subscriber) {
            var tcs = new TaskCompletionSource<Button>();
            var eventName = EventConsts.catUi + UiEvents.BUTTON_CLICKED;
            EventBus.instance.Subscribe(subscriber, eventName, (Button button) => {
                if (button.HasComponent<Link>(out var link) && link.id == targetId) {
                    var unsubscribed = EventBus.instance.Unsubscribe(subscriber, eventName);
                    AssertV2.IsTrue(unsubscribed, "unsubscribed event listener");
                    tcs.SetResult(button);
                }
            });
            var waitForToggleToBeChecked = tcs.Task;
            return waitForToggleToBeChecked;
        }

        public static Task<Toggle> WaitForToggle(string targetId, bool shouldBeChecked) {
            return WaitForToggle(targetId, shouldBeChecked, new object());
        }

        public static Task<Toggle> WaitForToggle(string targetId, bool shouldBeChecked, object subscriber) {
            var tcs = new TaskCompletionSource<Toggle>();
            var eventName = EventConsts.catUi + UiEvents.TOGGLE_CHANGED;
            EventBus.instance.Subscribe(subscriber, eventName, (Toggle toggle, bool isChecked) => {
                var isCorrectToggle = toggle.HasComponent<Link>(out var link) && link.id == targetId;
                if (isCorrectToggle && isChecked == shouldBeChecked) {
                    var unsubscribed = EventBus.instance.Unsubscribe(subscriber, eventName);
                    AssertV2.IsTrue(unsubscribed, "unsubscribed event listener");
                    tcs.SetResult(toggle);
                }
            });
            var waitForToggleToBeChecked = tcs.Task;
            return waitForToggleToBeChecked;
        }

        public static Task<InputField> WaitForInputField(string targetId, Func<string, bool> isTextAccepted) {
            return WaitForInputField(targetId, isTextAccepted, new object());
        }

        public static Task<InputField> WaitForInputField(string targetId, Func<string, bool> isTextAccepted, object subscriber) {
            var tcs = new TaskCompletionSource<InputField>();
            var eventName = EventConsts.catUi + UiEvents.INPUTFIELD_CHANGED;
            EventBus.instance.Subscribe(subscriber, eventName, (InputField input, string newText) => {
                var isCorrectInput = input.HasComponent<Link>(out var link) && link.id == targetId;
                if (isCorrectInput && isTextAccepted(newText)) {
                    var unsubscribed = EventBus.instance.Unsubscribe(subscriber, eventName);
                    AssertV2.IsTrue(unsubscribed, "unsubscribed event listener");
                    tcs.SetResult(input);
                }
            });
            var waitForToggleToBeChecked = tcs.Task;
            return waitForToggleToBeChecked;
        }

        public static Task<Dropdown> WaitForDropDown(string targetId, Func<int, bool> isTextAccepted) {
            return WaitForDropDown(targetId, isTextAccepted, new object());
        }

        public static Task<Dropdown> WaitForDropDown(string targetId, Func<int, bool> isTextAccepted, object subscriber) {
            var tcs = new TaskCompletionSource<Dropdown>();
            var eventName = EventConsts.catUi + UiEvents.DROPDOWN_CHANGED;
            EventBus.instance.Subscribe(subscriber, eventName, (Dropdown dropdown, int selection) => {
                var isCorrectInput = dropdown.HasComponent<Link>(out var link) && link.id == targetId;
                if (isCorrectInput && isTextAccepted(selection)) {
                    var unsubscribed = EventBus.instance.Unsubscribe(subscriber, eventName);
                    AssertV2.IsTrue(unsubscribed, "unsubscribed event listener");
                    tcs.SetResult(dropdown);
                }
            });
            var waitForToggleToBeChecked = tcs.Task;
            return waitForToggleToBeChecked;
        }

    }

}