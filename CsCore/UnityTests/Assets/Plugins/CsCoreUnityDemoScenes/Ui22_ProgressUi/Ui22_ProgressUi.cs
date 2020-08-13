using com.csutil.progress;
using com.csutil.ui;
using System.Collections;
using UnityEngine.UI;

namespace com.csutil.tests {

    public class Ui22_ProgressUi : UnitTestMono {

        public double totalCount = 10;

        public override IEnumerator RunTest() {

            var pm = new ProgressManager();
            gameObject.GetComponentInChildren<ProgressUi>().progressManager = pm;

            var map = gameObject.GetLinkMap();

            int id = 0;

            map.Get<Button>("NewProgress").SetOnClickAction(delegate {
                id++;
                pm.GetOrAddProgress("Progress:" + id, totalCount, true);
            });

            map.Get<Button>("IncrementCurrentProgress").SetOnClickAction(delegate {
                pm.GetProgress("Progress:" + id).IncrementCount();
            });

            yield return null;

        }

    }

}