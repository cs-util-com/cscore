using System.Threading.Tasks;
using com.csutil.math;
using TMPro;
using UnityEngine;

namespace com.csutil.settings {
    
    public abstract class SettingsUi<T> : Presenter<T> {

        public GameObject targetView { get; set; }

        public Task OnLoad(T model) {
            var links = targetView.GetLinkMap();
            var fpsText = links.Get<TMP_Text>("CurrentFps");
            MonitorCurrentFps(fpsText);

            RefreshSettingsUi(model);
            return Task.CompletedTask;
        }
        protected abstract void RefreshSettingsUi(T model);

        private void MonitorCurrentFps(TMP_Text fpsText) {
            var fpsHistoryList = new FixedSizedQueue<int>(size: 20);
            fpsText.ExecuteRepeated(() => {
                var newFps = (int)(1f / Time.unscaledDeltaTime);
                fpsHistoryList.Enqueue(newFps);
                var medianFps = fpsHistoryList.CalcMedian();
                fpsText.text = medianFps + " FPS";
                return true;
            }, delayInMsBetweenIterations: 100);
        }

    }
    
}