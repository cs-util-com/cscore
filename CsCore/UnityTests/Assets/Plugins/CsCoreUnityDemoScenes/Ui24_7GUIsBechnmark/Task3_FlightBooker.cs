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
            public DateTime tripStart = DateTimeV2.Now;
            public DateTime tripBack;

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

                tripStartInput.text = "" + DateTimeV2.Now;
                tripStartInput.AddOnValueChangedActionThrottled(newVal => {
                    if (ShowValidInUi(tripBackInput, DateTime.TryParse(newVal, out DateTime x))) {
                        model.tripStart = x;
                        Validate(model);
                    }
                }, delayInMs: 1000);
                tripBackInput.AddOnValueChangedActionThrottled(newVal => {
                    if (ShowValidInUi(tripBackInput, DateTime.TryParse(newVal, out DateTime x))) {
                        model.tripBack = x;
                        Validate(model);
                    }
                }, delayInMs: 1000);
                oneWayDropdown.SetOnValueChangedAction(selectedEntry => {
                    Log.e("rejected selectedEntry=" + selectedEntry);
                    return false;
                });
                return bookButton.SetOnClickAction(delegate {
                    Log.d("Flight now booked: " + JsonWriter.AsPrettyString(model));
                });
            }

            private void Validate(MyModel model) {
                bool startValid = ShowValidInUi(tripStartInput, model.tripStart > DateTime.Now);
                bool backValid = ShowValidInUi(tripBackInput, model.tripBack > model.tripStart);
                bookButton.enabled = startValid && backValid;
            }

            private bool ShowValidInUi(InputField i, bool valid) {
                var c = valid ? ThemeColor.ColorNames.elementContrast : ThemeColor.ColorNames.warning;
                Log.MethodEnteredWith(i, valid, c);
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