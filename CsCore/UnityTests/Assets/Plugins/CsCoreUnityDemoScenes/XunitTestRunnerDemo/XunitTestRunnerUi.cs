using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using com.csutil.tests;
using ReuseScroller;

namespace com.csutil.testing {

    public class XunitTestRunnerUi : BaseController<XunitTestRunner.Test> {

        private const int timeoutInMs = 30000;

        public bool autoRunAllTests = false;

        protected override void Start() {
            base.Start();
            var links = GetComponentInParent<Canvas>().gameObject.GetLinkMap();
            var autoRunToggle = links.Get<Toggle>("AutoRunToggle");
            autoRunToggle.isOn = autoRunAllTests;
            autoRunToggle.SetOnValueChangedAction(isChecked => {
                autoRunAllTests = isChecked;
                return true;
            });
            links.Get<Button>("StartButton").SetOnClickAction((button) => { CollectTests(links); });
        }

        private void CollectTests(Dictionary<string, Link> links) {
            var allClasses = typeof(MathTests).Assembly.GetExportedTypes();
            var allTests = XunitTestRunner.CollectAllTests(allClasses, delegate {
                //// setup before each test, use same error collector for all tests
            });
            AssertV2.AreNotEqual(0, allTests.Count);
            StartCoroutine(RunAllTests(allTests));
            if (autoRunAllTests) {
                links.Get<Text>("ButtonText").text = "Now running " + allTests.Count + " tests..";
            } else {
                links.Get<Text>("ButtonText").text = "Found " + allTests.Count + " tests (click one to run it)";
            }
        }

        private IEnumerator RunAllTests(List<XunitTestRunner.Test> allTests) {
            this.CellData = allTests;
            if (autoRunAllTests) {
                foreach (var test in allTests) { yield return RunTestCoroutine(test); }
                AssertV2.AreEqual(0, allTests.Filter(t => t.testTask.IsFaulted).Count());
            }
        }

        internal void RunTest(XunitTestRunner.Test test) { StartCoroutine(RunTestCoroutine(test)); }

        private IEnumerator RunTestCoroutine(XunitTestRunner.Test test) {
            test.StartTest();
            ReloadData();
            yield return new WaitForEndOfFrame();
            yield return test.testTask.AsCoroutine((e) => { Debug.LogError(e); }, timeoutInMs);
            ReloadData();
            yield return new WaitForEndOfFrame();
        }

    }

}