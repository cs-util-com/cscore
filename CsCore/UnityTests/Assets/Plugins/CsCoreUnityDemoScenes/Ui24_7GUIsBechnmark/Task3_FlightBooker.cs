using com.csutil.ui;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    internal static class Task3_FlightBooker {

        public static async Task ShowIn(ViewStack viewStack) {
            var model = new MyModel();
            var presenter = new MyPresenter();
            presenter.targetView = viewStack.ShowView("7GUIs_Task3_FlightBooker");
            await presenter.LoadModelIntoView(model);
        }

        private class MyModel {

            public enum FlightType { oneWayFlight, withReturnFlight }

            public FlightType flightType = FlightType.oneWayFlight;
            public DateTime tripStart = DateTimeV2.Now + TimeSpan.FromMinutes(60);
            public DateTime? tripBack;

        }

        private class MyPresenter : Presenter<MyModel> {

            public GameObject targetView { get; set; }

            private InputField tripStartInput;
            private InputField tripBackInput;
            private Dropdown oneWayDropdown;
            private Button bookButton;

            public Task OnLoad(MyModel model) {
                var map = targetView.GetLinkMap();
                tripStartInput = map.Get<InputField>("TripStartInput");
                tripBackInput = map.Get<InputField>("TripBackInput");
                oneWayDropdown = map.Get<Dropdown>("OneWayDropdown");
                bookButton = map.Get<Button>("BookButton");

                tripStartInput.text = "" + model.tripStart;
                tripStartInput.AddOnValueChangedActionThrottled(newVal => {
                    if (ShowValidInUi(tripStartInput, DateTime.TryParse(newVal, out DateTime d))) {
                        model.tripStart = d;
                        UpdateUi(model);
                    }
                }, delayInMs: 1000);
                tripBackInput.AddOnValueChangedActionThrottled(newVal => {
                    if (ShowValidInUi(tripBackInput, DateTime.TryParse(newVal, out DateTime d))) {
                        model.tripBack = d;
                        UpdateUi(model);
                    }
                }, delayInMs: 1000);
                oneWayDropdown.SetOnValueChangedAction(selectedEntry => {
                    model.flightType = IsTwoWaySelected() ? MyModel.FlightType.withReturnFlight : MyModel.FlightType.oneWayFlight;
                    // Auto fill the trip back the first time its selected:
                    if (IsTwoWaySelected() && model.tripBack == null) {
                        model.tripBack = model.tripStart + TimeSpan.FromDays(1);
                        tripBackInput.text = "" + model.tripBack;
                    }
                    UpdateUi(model);
                    return true;
                });
                UpdateUi(model);
                return bookButton.SetOnClickAction(delegate {
                    Toast.Show("Flight now booked: " + JsonWriter.AsPrettyString(model));
                });
            }

            private void UpdateUi(MyModel model) {
                bool inputIsValid = ShowValidInUi(tripStartInput, model.tripStart > DateTime.Now);
                var isTwoWaySelected = IsTwoWaySelected();
                Log.MethodEnteredWith(oneWayDropdown.value);
                tripBackInput.interactable = isTwoWaySelected;
                if (isTwoWaySelected) {
                    inputIsValid = ShowValidInUi(tripBackInput, model.tripBack > model.tripStart) & inputIsValid;
                }
                bookButton.interactable = inputIsValid;
            }

            private bool IsTwoWaySelected() { return oneWayDropdown.value == 1; }

            private bool ShowValidInUi(InputField i, bool valid) {
                var c = valid ? ThemeColor.ColorNames.elementContrast : ThemeColor.ColorNames.warning;
                i.SetNormalColor(IoC.inject.Get<Theme>(this).GetColor(c));
                return valid;
            }

        }

        private static void SetNormalColor(this InputField self, Color color) {
            var colors = self.colors;
            colors.normalColor = color;
            colors.highlightedColor = color;
            colors.selectedColor = color;
            self.colors = colors;
        }

    }

}