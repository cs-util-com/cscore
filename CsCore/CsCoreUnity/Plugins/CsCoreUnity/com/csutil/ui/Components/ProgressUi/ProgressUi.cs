using UnityEngine;
using UnityEngine.UI;
using System;

namespace com.csutil.progress {

    public class ProgressUi : MonoBehaviour {

        public Slider progress;
        public Text progressText;
        public ProgressManager progressManager;

        private void OnEnable() {
            AssertV2.AreEqual(100, progress.maxValue, "progress.maxValue");
            this.ExecuteDelayed(delegate {
                if (progressManager == null) { progressManager = IoC.inject.Get<ProgressManager>(this); }
                if (progressManager == null) { throw new NullReferenceException("No ProgressManager available"); }
                progressManager.OnProgressUpdate += OnProgressUpdate;
                OnProgressUpdate(progressManager, null);
            }, delayInMsBeforeExecution: 100); // Wait for manager to exist
        }

        private void OnDisable() {
            if (progressManager != null) { progressManager.OnProgressUpdate -= OnProgressUpdate; }
        }

        private void OnProgressUpdate(object sender, IProgress _) {
            AssertV2.IsTrue(sender == progressManager, "sender != pm (ProgressManager field)");
            progressManager.CalcLatestStats();
            var percent = Math.Round(progressManager.combinedAvgPercent, 3);
            progress.value = (int)percent;
            if (progressText != null) {
                progressText.text = $"{percent}% ({progressManager.combinedCount}/{progressManager.combinedTotalCount})";
            }
        }

    }

}