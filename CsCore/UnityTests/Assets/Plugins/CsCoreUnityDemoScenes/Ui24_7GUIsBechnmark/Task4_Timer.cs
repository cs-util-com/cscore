using com.csutil.ui;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    internal static class Task4_Timer {

        public static async Task ShowIn(ViewStack viewStack) {
            var model = new MyModel();
            var presenter = new MyPresenter();
            presenter.targetView = viewStack.ShowView("7GUIs_Task4_Timer");
            await presenter.LoadModelIntoView(model);
        }

        private class MyModel {
            public bool timerRunning;
            public DateTime start;
            public float elapsedTimeInS;
            public float durationInS = 10;
        }

        private class MyPresenter : Presenter<MyModel> {

            public GameObject targetView { get; set; }
            private Slider elapsedTimeProgress;
            private Slider durationSlider;
            private Text timerText;
            private Text handleText;

            public Task OnLoad(MyModel model) {
                var map = targetView.GetLinkMap();
                elapsedTimeProgress = map.Get<Slider>("ElapsedTimeProgress");
                handleText = elapsedTimeProgress.handleRect.GetComponent<Text>();
                durationSlider = map.Get<Slider>("DurationSlider");
                timerText = map.Get<Text>("TimerLabel");

                RestartTimer(model);
                durationSlider.value = model.durationInS;
                durationSlider.SetOnValueChangedAction(newVal => {
                    model.durationInS = newVal;
                    RefreshUi(model);
                    return true;
                });
                return map.Get<Button>("ResetButton").SetOnClickAction(delegate { RestartTimer(model); });
            }

            private void RestartTimer(MyModel model) {
                model.start = DateTimeV2.Now;
                if (model.timerRunning) { return; }
                model.timerRunning = true;
                elapsedTimeProgress.ExecuteRepeated(() => {
                    model.elapsedTimeInS = (float)(DateTimeV2.Now - model.start).TotalSeconds;
                    RefreshUi(model);
                    model.timerRunning = model.elapsedTimeInS < model.durationInS;
                    return model.timerRunning; // Continue as long as elapsed time is smaller then duration
                }, delayInMsBetweenIterations: 50);
            }

            private void RefreshUi(MyModel model) {
                timerText.text = Math.Round(model.elapsedTimeInS, 1) + "s";
                elapsedTimeProgress.maxValue = model.durationInS;
                elapsedTimeProgress.value = model.durationInS - model.elapsedTimeInS;
                handleText.text = elapsedTimeProgress.value + "s";
            }

        }

    }

}