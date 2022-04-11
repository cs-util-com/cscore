using com.csutil.progress;
using com.csutil.ui;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    public class Ui22_ProgressUi : UnitTestMono {

        public double totalCount = 10;

        public override IEnumerator RunTest() {
            SetupGlobalProgressUi();
            SetupLocalProgressButton("ProgressButton");
            SetupLocalProgressButton("CircularProgressButton1");
            SetupProgressButtonForSimulatedProgress("CircularProgressButton2");
            SetupProgressButtonForSimulatedProgress("CircularProgressButton3");
            yield return SetupBlockingProgressButton().AsCoroutine();
        }

        /// <summary> The first example here demonstrates how a progress manager can be 
        /// created and then shown in a global progress UI (that is eg shown on top of all 
        /// screens). This manager can be set as a global singlton to use it from anywhere 
        /// in your logic and show overall progress indication of background tasks to the 
        /// user to give them a sense of the amount of work that is currently happening 
        /// and when it will approximatly be done. </summary>
        private void SetupGlobalProgressUi() {
            var pm1 = new ProgressManager();
            var uisInRootCanvas = RootCanvas.GetOrAddRootCanvasV2().gameObject.GetChildren();
            // Find the single one that has a progress UI attached to it and assume its the global process UI screen to be used:
            var globalProgressUi = uisInRootCanvas.Single(x => x.GetComponentV2<ProgressUi>() != null).GetComponentV2<ProgressUi>();
            globalProgressUi.progressManager = pm1;

            int id = 0;
            var map = gameObject.GetLinkMap();
            map.Get<Button>("NewProgress").SetOnClickAction(delegate {
                id++;
                pm1.GetOrAddProgress("Progress:" + id, totalCount, true);
            });
            map.Get<Button>("IncrementCurrentProgress").SetOnClickAction(delegate {
                pm1.GetProgress("Progress:" + id).IncrementCount();
            });
        }

        /// <summary> There can be multiple progress managers and they can be visualized on 
        /// multiple UIs each. The second example shows that a progress can be shown directly 
        /// on any UI element that has an image set up using the images fill amount </summary>
        private void SetupLocalProgressButton(string buttonName) {
            // Create an independent progress manager to be only shown on the button
            var pm2 = new ProgressManager();
            var button = gameObject.GetLinkMap().Get<Button>(buttonName);
            button.GetComponentInChildren<ProgressUiViaImage>().progressManager = pm2;
            button.SetOnClickAction(delegate {
                // Get the progress object (or create a new one if not found) and increment it:
                pm2.GetOrAddProgress("Progress on Button UI", 10, true).IncrementCount();
            });
        }

        private void SetupProgressButtonForSimulatedProgress(string buttonName) {
            // Create an independent progress manager to be only shown on the button
            var pm2 = new ProgressManager();
            var button = gameObject.GetLinkMap().Get<Button>(buttonName);
            ProgressUiViaImage progressUi = button.GetComponentInChildren<ProgressUiViaImage>();
            progressUi.progressManager = pm2;
            button.SetOnClickAction(async delegate {
                // Get the progress object (or create a new one if not found) and increment it:
                var progress = pm2.GetOrAddProgress("Simulated Progress at " + Time.realtimeSinceStartup, 10, true);
                await SimulateProgressOn(progressUi, progress, 400);
            });
        }

        /// <summary> Displays a progress UI that is blocking so that the user cant do input 
        /// while the progress is showing, useful for loading screens etc </summary>
        private async Task SetupBlockingProgressButton() {
            await gameObject.GetLinkMap().Get<Button>("BlockingProgressButton").SetOnClickAction(async delegate {
                ProgressManager prManager = new ProgressManager();
                ProgressUi progressUi = gameObject.GetViewStack().ShowBlockingProgressUiFor(prManager);
                IProgress progress = prManager.GetOrAddProgress("DemoLoadingProgress", 200, true);
                await SimulateProgressOn(progressUi, progress, 15);
                AssertV2.IsTrue(progressUi.IsDestroyed(), "Blocking progress not destroyed after it completed");
            });
        }

        private static async Task SimulateProgressOn(ProgressUi progressUi, IProgress progress, int delayInMsPerUpdate) {
            progressUi.progressDetailsInfoText?.textLocalized("I am a progress UI, I hope I wont take to long!");
            for (int i = 0; i < progress.totalCount - 1; i++) {
                progress.IncrementCount();
                await TaskV2.Delay(delayInMsPerUpdate);
            }
            progressUi.progressDetailsInfoText?.textLocalized("The last % is always the hardest!");
            await TaskV2.Delay(2000);
            progress.IncrementCount();
            await TaskV2.Delay(15);
        }
    }

}