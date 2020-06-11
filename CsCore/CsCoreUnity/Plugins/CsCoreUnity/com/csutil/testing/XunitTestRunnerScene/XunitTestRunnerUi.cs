using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ReuseScroller;
using System.Reflection;
using System;

namespace com.csutil.testing {

    public class XunitTestRunnerUi : BaseController<XunitTestRunner.Test> {

        private const int timeoutInMs = 30000;

        public bool autoRunAllTests = false;
        public List<string> anyTypeInTargetAssembly = new List<string>() {
            "e.g. MyNamespace.MyClass1, MyAssembly1",
            "com.csutil.tests.MathTests, CsCoreXUnitTests"
        };
        private List<XunitTestRunner.Test> allTests;

        protected override void Start() {
            base.Start();

            var assembliesToTest = anyTypeInTargetAssembly.Map(typeString => {
                try { return Type.GetType(typeString).Assembly; }
                catch (Exception) {
                    Log.e("Could not find type for string '" + typeString + "'");
                    return null;
                }
            });

            // On the parent canvas level collect all links:
            var links = GetComponentInParent<Canvas>().gameObject.GetLinkMap();
            var autoRunToggle = links.Get<Toggle>("AutoRunToggle");
            autoRunToggle.isOn = autoRunAllTests;
            autoRunToggle.SetOnValueChangedAction(isChecked => {
                autoRunAllTests = isChecked;
                return true;
            });
            links.Get<Button>("StartButton").SetOnClickAction((_) => {
                CollectTests(assembliesToTest, links);
                UpdateSearchFilter(links);
            });
            links.Get<InputField>("SearchInput").SetOnValueChangedActionThrottled((_) => {
                UpdateSearchFilter(links);
            }, 200);
        }

        private void UpdateSearchFilter(Dictionary<string, Link> links) {
            var newSearchText = links.Get<InputField>("SearchInput").text;
            newSearchText = newSearchText.ToLowerInvariant();
            if (allTests.IsNullOrEmpty()) { return; }
            CellData = allTests.Filter(t => t.name.ToLowerInvariant().Contains(newSearchText)).ToList();
        }

        private void CollectTests(IEnumerable<Assembly> assembliesToTest, Dictionary<string, Link> links) {
            var allClasses = assembliesToTest.SelectMany(assembly => {
                if (assembly == null) { return new Type[0]; } // return emtpy list
                return assembly.GetExportedTypes();
            });
            allTests = XunitTestRunner.CollectAllTests(allClasses, (test) => {
                // callback before a test is executed
            });
            AssertV2.AreNotEqual(0, allTests.Count);
            StartCoroutine(ShowAllFoundTests(allTests));
            if (autoRunAllTests) {
                links.Get<Text>("ButtonText").text = "Now running " + allTests.Count + " tests..";
            } else {
                links.Get<Text>("ButtonText").text = "Found " + allTests.Count + " tests (click one to run it)";
            }
        }

        private IEnumerator ShowAllFoundTests(List<XunitTestRunner.Test> allTests) {
            this.CellData = allTests;
            if (autoRunAllTests) {
                foreach (var test in allTests) { yield return RunTestCoroutine(test); }
                AssertV2.AreEqual(0, allTests.Filter(t => t.testTask.IsFaulted).Count());
            }
        }

        public void RunTest(XunitTestRunner.Test test) { StartCoroutine(RunTestCoroutine(test)); }

        private IEnumerator RunTestCoroutine(XunitTestRunner.Test test) {
            test.StartTest();
            ReloadData();
            yield return new WaitForEndOfFrame();
            yield return test.testTask.AsCoroutine((e) => { Log.e(e); }, timeoutInMs);
            ReloadData();
            yield return new WaitForEndOfFrame();
        }

    }

}