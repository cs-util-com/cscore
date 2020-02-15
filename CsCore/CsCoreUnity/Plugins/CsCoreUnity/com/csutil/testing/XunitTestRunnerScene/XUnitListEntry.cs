using ReuseScroller;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.testing {

    /// <summary> 
    /// Has to be used in a Prefab that is then placed in the XunitTestRunnerUi CellObject field 
    /// </summary>
    public class XUnitListEntry : BaseCell<XunitTestRunner.Test> {

        public Text testName;
        public Text testStatus;
        public Button entryButton;
        public Image entryBackground;

        public override void UpdateContent(XunitTestRunner.Test test) {
            entryButton.SetOnClickAction(delegate {
                GetComponentInParent<XunitTestRunnerUi>().RunTest(test);
            });
            testName.text = test.name;
            if (test.testTask == null) {
                testStatus.text = "Not started yet";
                entryBackground.color = Color.white;
            } else if (test.testTask.IsFaulted) {
                var error = test.testTask.Exception;
                testStatus.text = "Error: " + error;
                entryBackground.color = Color.red;
            } else if (test.testTask.IsCompleted) {
                testStatus.text = "Passed";
                entryBackground.color = Color.green;
            } else {
                testStatus.text = "Running..";
                entryBackground.color = Color.blue;
            }
        }

    }

}