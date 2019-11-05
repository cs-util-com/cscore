using com.csutil.logging;
using com.csutil.ui;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests.ui {

    // This MonoBehaviour can be used for manual tests by attaching it to a scene
    public class Ui8_LogConsole : MonoBehaviour { IEnumerator Start() { yield return new Ui8_LogConsoleTests().ExampleUsage(); } }

    // The automated unit test that is called by the MonoBehaviour or by the Unity Test Runner
    class Ui8_LogConsoleTests {
        [UnityTest]
        public IEnumerator ExampleUsage() {

            LogConsole.RegisterForAllLogEvents(this);

            for (int i = 0; i < 20; i++) {
                LogConsole.GetLogConsole(this).AddToLog(LogEntry.d("Log event " + i));
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

}