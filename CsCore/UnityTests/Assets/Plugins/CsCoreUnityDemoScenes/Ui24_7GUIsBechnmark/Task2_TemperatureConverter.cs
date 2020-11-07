using com.csutil.ui;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    internal static class Task2_TemperatureConverter {

        public static async Task ShowIn(ViewStack viewStack) {
            var presenter = new MyPresenter();
            presenter.targetView = viewStack.ShowView("7GUIs_Task2_TemperatureConverter");
            await presenter.LoadModelIntoView(0); // Startvalue 0 Celsius
        }

        private class MyPresenter : Presenter<float> {

            public GameObject targetView { get; set; }
            public Task OnLoad(float startCelsius) {
                var map = targetView.GetLinkMap();
                InputField celsius = map.Get<InputField>("CelsiusInput");
                InputField fahrenheit = map.Get<InputField>("FahrenheitInput");
                celsius.AddOnValueChangedAction((newText) => {
                    if (float.TryParse(newText, out float x)) {
                        if (x < -273.15f) { return false; }
                        fahrenheit.text = "" + ToFahrenheit(x);
                        return true;
                    }
                    return false;
                });
                fahrenheit.AddOnValueChangedAction((newText) => {
                    if (float.TryParse(newText, out float x)) {
                        if (x < -459.67f) { return false; }
                        celsius.text = "" + ToCelsius(x);
                        return true;
                    }
                    return false;
                });
                celsius.SetTextLocalizedWithNotify("" + startCelsius);
                return Task.FromResult(true);
            }

            private float ToFahrenheit(float celsius) { return celsius * 9f / 5f + 32f; }
            private float ToCelsius(float fahrenheit) { return (fahrenheit - 32f) * 5f / 9f; }

        }

    }
}