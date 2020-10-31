using com.csutil.progress;
using UnityEngine;

namespace com.csutil.ui {

    public static class ProgressExtensions {

        public static ProgressUi ShowBlockingProgressUiFor(this ViewStack self, ProgressManager prManager) {
            return ShowBlockingProgressUiFor(self, "Progress/BlockingProgressOverlay1", prManager);
        }

        private static ProgressUi ShowBlockingProgressUiFor(ViewStack self,
                                        string blockingProgressViewPrefabName, ProgressManager prManager) {
            GameObject prGo = self.ShowView(blockingProgressViewPrefabName);
            ProgressUi prUi = prGo.GetComponentInChildren<ProgressUi>();
            prUi.onProgressUiComplete += () => { prGo.GetViewStack().SwitchBackToLastView(prGo); };
            prUi.progressManager = prManager;
            return prUi;
        }

    }

}