using com.csutil.ui;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    internal static class Task1_Counter {

        public static async Task ShowIn(ViewStack viewStack) {
            var model = new MyModel() { counter = 0 };
            var presenter = new MyPresenter();
            presenter.targetView = viewStack.ShowView("7GUIs_Task1_Counter");
            await presenter.LoadModelIntoView(model);
        }

        /// <summary> The model in the counter example is just a single number which
        /// is being increased every time the user clicks the Count button </summary>
        private class MyModel {
            public int counter = 0;
        }

        private class MyPresenter : Presenter<MyModel> {

            public GameObject targetView { get; set; }
            public Task OnLoad(MyModel model) {
                var map = targetView.GetLinkMap();
                InputField input = map.Get<InputField>("CountInput");
                input.text = "" + model.counter;
                return map.Get<Button>("CountButton").SetOnClickAction(delegate {
                    model.counter += 1;
                    input.text = "" + model.counter;
                });
            }

        }

    }

}