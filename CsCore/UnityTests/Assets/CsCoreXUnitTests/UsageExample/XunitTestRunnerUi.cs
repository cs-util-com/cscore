using com.csutil;
using com.csutil.testing;
using com.csutil.tests;
using Mopsicus.InfiniteScroll;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class XunitTestRunnerUi : MonoBehaviour {

    public int defaultEntryHeight = 80;
    public int timeoutInMs = 30000;
    private List<XunitTestRunner.Test> startedTests = new List<XunitTestRunner.Test>();

    private void OnEnable() {
        var links = gameObject.GetLinkMap();
        var listUi = links.Get<InfiniteScroll>("HorizontalScrollView");
        listUi.OnHeight += OnHeight;
        listUi.OnFill += OnFill;
        links.Get<Button>("StartButton").SetOnClickAction(delegate { StartCoroutine(RunAllTests(listUi)); });
    }

    private int OnHeight(int index) {
        return defaultEntryHeight;
    }

    private void OnFill(int pos, GameObject view) {
        var test = startedTests[pos];
        var links = view.GetLinkMap();
        links.Get<Text>("Name").text = test.methodToTest.ToStringV2();
        if (test.testTask.IsFaulted) {
            links.Get<Text>("Status").text = "Error: " + test.reportedError;
            links.Get<Image>("StatusColor").color = Color.red;
            Log.e("" + test.reportedError);
        } else if (test.testTask.IsCompleted) {
            links.Get<Text>("Status").text = "Passed";
            links.Get<Image>("StatusColor").color = Color.green;
        } else {
            links.Get<Text>("Status").text = "Running..";
            links.Get<Image>("StatusColor").color = Color.blue;
        }
    }

    public IEnumerator RunAllTests(InfiniteScroll listUi) {
        var errorCollector = new LogForXunitTestRunnerInUnity();
        var allClasses = typeof(MathTests).Assembly.GetExportedTypes();
        foreach (var classToTest in allClasses) {
            yield return new WaitForEndOfFrame();
            var testIterator = XunitTestRunner.CreateExecutionIterator(classToTest, delegate {
                //// setup before each test, use same error collector for all tests:
                Log.instance = errorCollector;
            });
            foreach (var startedTest in testIterator) {
                // The test now started:
                startedTests.Add(startedTest);

                try {
                    if (startedTests.Count == 1) {
                        listUi.InitData(1);
                    } else {
                        // Trigger add next test to UI:
                        listUi.ApplyDataTo(startedTests.Count - 1, startedTests.Count, InfiniteScroll.Direction.Bottom);
                    }
                }
                catch (Exception e) { Log.w("" + e); }

                yield return new WaitForEndOfFrame();
                var t = Log.MethodEntered("Now running test " + startedTest);
                yield return startedTest.testTask.AsCoroutine((e) => { Debug.LogError(e); }, timeoutInMs);
                Log.MethodDone(t);
                yield return new WaitForEndOfFrame();

                listUi.UpdateVisible();
            }
        }

        AssertV2.AreEqual(0, startedTests.Filter(t => t.testTask.IsFaulted).Count());

    }

}
