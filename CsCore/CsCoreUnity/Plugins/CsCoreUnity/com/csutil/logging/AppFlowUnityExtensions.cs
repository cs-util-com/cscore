using com.csutil.ui;
using com.csutil.ui.viewstack;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil {

    public static class AppFlowUnityExtensions {

        public static T WithAllTrackingActive<T>(this T self) where T : IAppFlow {
            self.WithBasicTrackingActive();
            self.ActivateLinkMapTracking();
            self.ActivatePrefabLoadTracking();
            self.ActivateHighLevelUiEventTracking();
            self.ActivateLowLevelUiEventTracking();
            self.ActivateViewStackTracking();
            return self;
        }

        public static void ActivatePrefabLoadTracking(this IAppFlow self) {
            EventBus.instance.Subscribe(self, EventConsts.catTemplate, (GameObject prefab) => {
                self.TrackEvent(EventConsts.catTemplate, "Loaded_" + prefab.name, prefab);
            });
        }

        public static void ActivateLowLevelUiEventTracking(this IAppFlow self) {

            // Button UI tracking:
            EventBus.instance.Subscribe(self, EventConsts.catUi + UiEvents.BUTTON_CLICKED, (Button button) => {
                self.TrackEvent(EventConsts.catUi, UiEvents.BUTTON_CLICKED + "_" + button, button);
            });

            // Toggle UI tracking:
            EventBus.instance.Subscribe(self, EventConsts.catUi + UiEvents.TOGGLE_CHANGED, (Toggle toggle, bool isChecked) => {
                self.TrackEvent(EventConsts.catUi, UiEvents.TOGGLE_CHANGED + "_" + toggle + "_" + isChecked, toggle, isChecked);
            });

            { // InputField UI tracking:
                EventHandler<string> action = (input, newText) => {
                    self.TrackEvent(EventConsts.catUi, UiEvents.INPUTFIELD_CHANGED + "_" + input, input);
                };
                var delayedAction = action.AsThrottledDebounce(delayInMs: 1900, skipFirstEvent: true);
                EventBus.instance.Subscribe(self, EventConsts.catUi + UiEvents.INPUTFIELD_CHANGED, (InputField input, string newText) => {
                    delayedAction(input, newText);
                });
            }

            // Dropdown UI tracking:
            EventBus.instance.Subscribe(self, EventConsts.catUi + UiEvents.DROPDOWN_CHANGED, (Dropdown d, int selection) => {
                self.TrackEvent(EventConsts.catUi, UiEvents.DROPDOWN_CHANGED + "_" + d + "_" + selection, d, selection);
            });

        }

        public static void ActivateHighLevelUiEventTracking(this IAppFlow self) {

            // ActionMenu tracking:
            EventBus.instance.Subscribe(self, EventConsts.catUi + UiEvents.ACTION_MENU, (string entry) => {
                self.TrackEvent(EventConsts.catUi, UiEvents.ACTION_MENU + "_" + entry, entry);
            });

            // Dialog tracking:
            EventBus.instance.Subscribe(self, EventConsts.catUi + UiEvents.DIALOG, (Dialog d) => {
                self.TrackEvent(EventConsts.catUi, UiEvents.DIALOG + "_" + d.caption, d);
            });
            EventBus.instance.Subscribe(self, EventConsts.catUi + UiEvents.CONFIRM_CANCEL_DIALOG, (ConfirmCancelDialog d) => {
                var action = UiEvents.CONFIRM_CANCEL_DIALOG + "_" + d.caption + (d.dialogWasConfirmed ? " CONFIRMED" : " CANCELED");
                self.TrackEvent(EventConsts.catUi, action, d);
            });
            EventBus.instance.Subscribe(self, EventConsts.catUi + UiEvents.INPUT_DIALOG, (ConfirmCancelDialog d) => {
                var action = UiEvents.INPUT_DIALOG + "_" + d.caption + (d.dialogWasConfirmed ? " CONFIRMED" : " CANCELED");
                self.TrackEvent(EventConsts.catUi, action, d);
            });
        }

        public static void ActivateViewStackTracking(this IAppFlow self) {

            EventBus.instance.Subscribe(self, EventConsts.catView + EventConsts.SHOW, (GameObject view) => {
                self.TrackEvent(EventConsts.catView, EventConsts.SHOW + "_" + view.name, view);
            });
            EventBus.instance.Subscribe(self, EventConsts.catView + EventConsts.SWITCH_BACK_TO_LAST, (string currentViewName, GameObject lastView) => {
                self.TrackEvent(EventConsts.catView, EventConsts.SWITCH_BACK_TO_LAST + "_" + currentViewName + "->" + lastView.name, lastView);
            });
            EventBus.instance.Subscribe(self, EventConsts.catView + EventConsts.SWITCH_TO_NEXT, (GameObject currentView, GameObject nextView) => {
                self.TrackEvent(EventConsts.catView, EventConsts.SWITCH_TO_NEXT + "_" + currentView.name + "->" + nextView.name, currentView, nextView);
            });
            EventBus.instance.Subscribe(self, EventConsts.catView + EventConsts.SWITCH_REJECTED, (DefaultSwitchScreenAction.SwitchDirection direction) => {
                self.TrackEvent(EventConsts.catView, EventConsts.SWITCH_REJECTED, direction);
            });
            EventBus.instance.Subscribe(self, EventConsts.catView + EventConsts.ADDED, (GameObject view) => {
                self.TrackEvent(EventConsts.catView, EventConsts.ADDED + "_" + view.name, view);
            });
            EventBus.instance.Subscribe(self, EventConsts.catView + EventConsts.REMOVED, (GameObject view) => {
                self.TrackEvent(EventConsts.catView, EventConsts.REMOVED + "_" + view.name, view);
            });

        }

    }

}
