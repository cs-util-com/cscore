using com.csutil.ui;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    internal static class Task2_TemperatureConverter {

        public static async Task ShowIn(ViewStack viewStack) {
            var model = new MyModel() { degreeInCelsius = 0 };
            var presenter = new MyPresenter();
            presenter.targetView = viewStack.ShowView("7GUIs_Task2_TemperatureConverter");
            await presenter.LoadModelIntoView(model);
        }

        /// <summary> The model in the counter example is just a single number which
        /// is the temperature stored in celsius unit in the model </summary>
        private class MyModel {
            public float degreeInCelsius = 0; // Startvalue 0 Celsius
        }

        private class MyPresenter : Presenter<MyModel> {

            public GameObject targetView { get; set; }
            public Task OnLoad(MyModel model) {
                var map = targetView.GetLinkMap();
                InputField celsiusInput = map.Get<InputField>("CelsiusInput");
                InputField fahrenheitInput = map.Get<InputField>("FahrenheitInput");
                celsiusInput.AddOnValueChangedAction((newText) => {
                    if (float.TryParse(newText, out float celsius)) {
                        if (celsius < -273.15f) { return false; }
                        model.degreeInCelsius = celsius; // Update model
                        fahrenheitInput.text = "" + ToFahrenheit(model.degreeInCelsius);
                        return true;
                    }
                    return false;
                });
                fahrenheitInput.AddOnValueChangedAction((newText) => {
                    if (float.TryParse(newText, out float fahrenheit)) {
                        if (fahrenheit < -459.67f) { return false; }
                        model.degreeInCelsius = ToCelsius(fahrenheit); // Update model
                        celsiusInput.textLocalized("" + model.degreeInCelsius);
                        return true;
                    }
                    return false;
                });
                celsiusInput.SetTextLocalizedWithNotify("" + model.degreeInCelsius);
                return Task.FromResult(true);
            }

            private float ToFahrenheit(float celsius) { return celsius * 9f / 5f + 32f; }
            private float ToCelsius(float fahrenheit) { return (fahrenheit - 32f) * 5f / 9f; }

        }

    }
}