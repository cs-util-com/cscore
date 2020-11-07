using com.csutil.ui;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    internal static class Task6_CircleDrawer {

        public static async Task ShowIn(ViewStack viewStack) {
            var model = new MyModel() { counter = 0 };
            var presenter = new MyPresenter();
            presenter.targetView = viewStack.ShowView("7GUIs_Task6_CircleDrawer");
            await presenter.LoadModelIntoView(model);
        }

        private class MyModel { public int counter = 0; }

        private class MyPresenter : Presenter<MyModel> {

            public GameObject targetView { get; set; }
            public Task OnLoad(MyModel model) {
                var map = targetView.GetLinkMap();
                return map.Get<Button>("Count").SetOnClickAction(delegate {

                });
            }

        }

    }

}