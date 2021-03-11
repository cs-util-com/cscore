using UnityEngine;
using UnityEngine.UI;
using System;
using com.csutil.ui;
using UnityEngine.Events;

namespace com.csutil.progress {

    public abstract class ProgressUi : MonoBehaviour {

        public static IProgress NewProgress(double totalCount) {
            return NewProgress(totalCount, "" + Guid.NewGuid());
        }

        public static IProgress NewProgress(double totalCount, string id) {
            var progressUi = IoC.inject.Get<ProgressUi>(id, false);
            if (progressUi == null) {
                progressUi = NewGlobalProgressUi(new ProgressManager());
                IoC.inject.SetSingleton(progressUi);
            }
            return progressUi.progressManager.GetOrAddProgress(id, totalCount, true);
        }

        private static ProgressUi NewGlobalProgressUi(ProgressManager pm, string prefab = "Progress/GlobalProgressOverlay1") {
            ProgressUi progressUi;
            var go = RootCanvas.GetOrAddRootCanvas().gameObject.AddChild(ResourcesV2.LoadPrefab(prefab));
            progressUi = go.GetComponentInChildren<ProgressUi>();
            progressUi.progressManager = pm;
            return progressUi;
        }

        /// <summary> Optional text that will show the current progress values </summary>
        public Text progressText;
        /// <summary> An optional info text that can be used to show the user details about what is happening </summary>
        public Text progressDetailsInfoText;
        /// <summary> If true will look for a CanvasGroupFader in parents to fade based on total progress </summary>
        public bool enableProgressUiFading = true;
        /// <summary> After this delay the finished progress will be cleared once all progresses are 100 so 
        /// that a clean start can be visualized once the next wave of progresses happen. Disabled if < 0 </summary>
        public int delayInMsBeforeProgressCleanup = 2000;

        public UnityAction onProgressUiComplete;

        /// <summary> The progress manage the UI will use as the source, will try to inject if not set manually </summary>
        public ProgressManager progressManager;

        private CanvasGroupFader canvasGroupFader;

        private void OnEnable() {
            SetPercentInUi(0);
            this.ExecuteDelayed(RegisterWithProgressManager, delayInMsBeforeExecution: 100); // Wait for manager to exist
        }

        private void RegisterWithProgressManager() {
            if (progressManager == null) { progressManager = IoC.inject.Get<ProgressManager>(this); }
            if (progressManager == null) { throw new NullReferenceException("No ProgressManager available"); }
            progressManager.OnProgressUpdate += OnProgressUpdate;
            OnProgressUpdate(progressManager, null);
        }

        private void OnDisable() {
            if (progressManager != null) { progressManager.OnProgressUpdate -= OnProgressUpdate; }
        }

        private void OnProgressUpdate(object sender, IProgress _) {
            AssertV2.IsTrue(sender == progressManager, "sender != pm (ProgressManager field)");
            var percent = Math.Round(progressManager.combinedAvgPercent, 3);
            if (progressText != null) {
                progressText.text = $"{percent}% ({progressManager.combinedCount}/{progressManager.combinedTotalCount})";
            }
            SetPercentInUi(percent);
            if (percent >= 100 && delayInMsBeforeProgressCleanup >= 0) {
                this.ExecuteDelayed(ResetProgressManagerIfAllFinished, delayInMsBeforeProgressCleanup);
                onProgressUiComplete?.Invoke();
            }
        }

        private void SetPercentInUi(double percent) {
            UpdateUiPercentValue(percent);
            // Handle progress UI fading:
            if (enableProgressUiFading) {
                if (canvasGroupFader == null) {
                    canvasGroupFader = GetProgressUiGo().GetComponentInParents<CanvasGroupFader>();
                    canvasGroupFader.ThrowErrorIfNull("canvasGroupFader");
                    canvasGroupFader.GetCanvasGroup().alpha = 0;
                }
                if (percent == 0 || percent >= 100) {
                    canvasGroupFader.targetAlpha = 0;
                } else {
                    canvasGroupFader.targetAlpha = canvasGroupFader.initialAlpha;
                }
            }
        }

        /// <summary> If currently all progresses of the manager are finished will remove all of them </summary>
        private void ResetProgressManagerIfAllFinished() {
            if (progressManager.combinedPercent >= 100) {
                progressManager.RemoveProcesses(progressManager.GetCompletedProgresses());
            }
        }

        protected abstract GameObject GetProgressUiGo();

        /// <summary> Called whenever the progress changes </summary>
        /// <param name="percent"> A value from 0 to 100 </param>
        protected abstract void UpdateUiPercentValue(double percent);

    }

}