using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ReuseScroller;
using System.Reflection;
using System;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.progress;
using Newtonsoft.Json;

namespace com.csutil.testing {

    public class XunitTestRunnerUi : BaseController<XunitTestRunner.Test> {

        private const int timeoutInMs = 90000;

        public bool autoRunAllTests = false;
        public List<string> anyTypeInTargetAssembly = new List<string>() {
            "e.g. MyNamespace.MyClass1, MyAssembly1",
            "com.csutil.tests.MathTests, CsCoreXUnitTests"
        };
        private IEnumerable<XunitTestRunner.Test> allTests;
        private ProgressManager pm;
        private FileBasedKeyValueStore _historyStore;

        protected override void Start() {
            base.Start();

            _historyStore = new FileBasedKeyValueStore(EnvironmentV2.instance.GetOrAddAppDataFolder("cscore XunitTestRunnerUi Test Execution History"));

            var assembliesToTest = anyTypeInTargetAssembly.Map(typeString => {
                try { return Type.GetType(typeString).Assembly; } catch (Exception) {
                    Log.e("Please check the XunitTestRunnerUi.anyTypeInTargetAssembly " +
                        "list in your scene UI, it if's configured correctly. Could " +
                        "not find type for string '" + typeString + "'", gameObject);
                    return null;
                }
            });

            pm = new ProgressManager();
            var progressUis = ResourcesV2.FindAllInScene<ProgressUi>();
            AssertV3.IsFalse(progressUis.IsNullOrEmpty(), () => "progressUis is null or empty");
            foreach (var progrUi in progressUis) { progrUi.progressManager = pm; }

            // On the parent canvas level collect all links:
            var links = GetComponentInParent<Canvas>().gameObject.GetLinkMap();
            var autoRunToggle = links.Get<Toggle>("AutoRunToggle");
            var hidePassedTestsToggle = GetHidePassedTestsToggle(links);

            autoRunToggle.isOn = autoRunAllTests;
            autoRunToggle.SetOnValueChangedAction(isChecked => {
                autoRunAllTests = isChecked;
                return true;
            });
            hidePassedTestsToggle.SetOnValueChangedAction(isChecked => {
                UpdateSearchFilter(links);
                return true;
            });
            links.Get<Button>("StartButton").SetOnClickAction(async (_) => {
                await CollectTests(assembliesToTest, links);
                UpdateSearchFilter(links);
            });
            links.Get<InputField>("SearchInput").SetOnValueChangedActionThrottled((_) => {
                UpdateSearchFilter(links);
            }, 200);
        }
        private static Toggle GetHidePassedTestsToggle(Dictionary<string, Link> links) { return links.Get<Toggle>("HidePassedTestsToggle"); }

        private void UpdateSearchFilter(Dictionary<string, Link> links) {
            var newSearchText = links.Get<InputField>("SearchInput").text;
            var hidePassedTests = GetHidePassedTestsToggle(links).isOn;
            newSearchText = newSearchText.ToLowerInvariant();
            if (allTests.IsNullOrEmpty()) { return; }
            CellData = allTests.Filter(t => t.name.ToLowerInvariant().Contains(newSearchText) && (!hidePassedTests || !t.IsCompletedSuccessfull())).ToList();
        }

        private async Task CollectTests(IEnumerable<Assembly> assembliesToTest, Dictionary<string, Link> links) {
            var allClasses = assembliesToTest.SelectMany(assembly => {
                if (assembly == null) { return new Type[0]; } // return emtpy list
                return assembly.GetExportedTypes();
            });
            allTests = XunitTestRunner.CollectAllTests(allClasses, (test) => {
                // callback before a test is executed
            });
            allTests = await SortByLastTestExecutionDuration(allTests);
            AssertV3.AreNotEqual(0, allTests.Count());
            StartCoroutine(ShowAllFoundTests(allTests));
            if (autoRunAllTests) {
                links.Get<Text>("ButtonText").text = "Now running " + allTests.Count() + " tests..";
            } else {
                links.Get<Text>("ButtonText").text = "Found " + allTests.Count() + " tests (click one to run it)";
            }
        }

        private async Task<IOrderedEnumerable<XunitTestRunner.Test>> SortByLastTestExecutionDuration(IEnumerable<XunitTestRunner.Test> tests) {
            var histories = await tests.MapAsync(x => _historyStore.Get<TestHistory>(x.name, null));
            var historyDict = histories.Filter(x => x != null).ToDictionary(x => x.Name, x => x);
            IOrderedEnumerable<XunitTestRunner.Test> result = tests.OrderBy(t => {
                if (historyDict.TryGetValue(t.name, out var entry)) {
                    if (!entry.IsCompletedSuccessfully) { return TestHistory.durationIfFailed; } // failed tests to the front
                    return entry.TestExecutionDurationInMs; // successful tests by duration (faster tests first)
                }
                return TestHistory.durationIfNeverRun; // tests that never where executed right after failed tests
            });
            return result;
        }

        private class TestHistory {

            /// <summary> failed tests to the front </summary>
            public const long durationIfFailed = -2;
            /// <summary> tests that never where executed right after failed tests </summary>
            public const long durationIfNeverRun = -1;
            /// <summary> Will be moved to the very end of the test execution to run as many tests as possible with each run </summary>
            public const long durationIfStartedButNeverFinished = long.MaxValue;

            public readonly string Name;
            public readonly long TestExecutionDurationInMs;
            public readonly bool IsCompletedSuccessfully;

            [JsonConstructor]
            public TestHistory(string name, long testExecutionDurationInMs, bool isCompletedSuccessfully) {
                Name = name;
                TestExecutionDurationInMs = testExecutionDurationInMs;
                IsCompletedSuccessfully = isCompletedSuccessfully;
            }

        }

        private IEnumerator ShowAllFoundTests(IEnumerable<XunitTestRunner.Test> allTests) {
            this.SetListData(allTests);
            if (autoRunAllTests) {
                using (var progress = pm.GetOrAddProgress("RunAllTests", allTests.Count(), true)) {
                    foreach (var test in allTests) {
                        progress.IncrementCount();
                        yield return RunTestCoroutine(test);
                    }
                }
                AssertV3.AreEqual(0, allTests.Filter(t => t.testTask.IsFaulted).Count());
            }
        }

        public void RunTest(XunitTestRunner.Test test) { StartCoroutine(RunTestCoroutine(test)); }

        private IEnumerator RunTestCoroutine(XunitTestRunner.Test test) {
            yield return _historyStore.Set(test.name, new TestHistory(test.name, TestHistory.durationIfStartedButNeverFinished, false)).AsCoroutine();
            var timing = StopwatchV2.StartNewV2();
            test.StartTest();
            ReloadData();
            yield return new WaitForEndOfFrame();
            yield return test.testTask.AsCoroutine((e) => { Log.e(e); }, timeoutInMs);
            timing.Stop();
            var historyEntry = new TestHistory(test.name, timing.ElapsedMilliseconds, test.IsCompletedSuccessfull());
            yield return _historyStore.Set(test.name, historyEntry).AsCoroutine();
            ReloadData();
            yield return new WaitForEndOfFrame();
        }

    }

}