using com.csutil.progress;
using System.Collections;
using UnityEngine.UI;

namespace com.csutil.tests {

    public class Ui22_ProgressUi : UnitTestMono {

        public double totalCount = 10;

        public override IEnumerator RunTest() {
            SetupGlobalProgressUi();
            SetupLocalProgressButton();
            yield return null;
        }

        /// <summary> THe first example here demonstrates how a progress manager can be 
        /// created and then shown in a global progress UI (that is eg shown on top of all 
        /// screens). This manager can be set as a global singlton to use it from anywhere 
        /// in your logic and show overall progress indication of background tasks to the 
        /// user to give them a sense of the amount of work that is currently happening 
        /// and when it will approximatly be done. </summary>
        private void SetupGlobalProgressUi() {
            var pm1 = new ProgressManager();
            gameObject.GetComponentInChildren<ProgressUi>().progressManager = pm1;

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
        private void SetupLocalProgressButton() {
            // Create a second independent progress manager to be only shown on the button
            var pm2 = new ProgressManager(); 
            var button = gameObject.GetLinkMap().Get<Button>("ProgressButton");
            button.GetComponentInChildren<ProgressUiViaImage>().progressManager = pm2;
            // Create a single progress that will be shown on the button once the user clicks it:
            IProgress progress = pm2.GetOrAddProgress("Progress on Button UI", 10, true);
            button.SetOnClickAction(delegate {
                progress.IncrementCount();
            });
        }

    }

}