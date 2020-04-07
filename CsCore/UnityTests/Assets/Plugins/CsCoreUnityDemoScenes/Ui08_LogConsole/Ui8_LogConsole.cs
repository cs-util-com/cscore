using com.csutil.logging;
using System.Collections;
using UnityEngine;

namespace com.csutil.tests.ui {

    public class Ui8_LogConsole : UnitTestMono {

        public override IEnumerator RunTest() {
            LogConsole.RegisterForAllLogEvents(this);
            for (int i = 0; i < 20; i++) {
                LogConsole.GetLogConsole(this).AddToLog(LogEntry.d("Log event " + i));
                yield return new WaitForSeconds(0.5f);
            }
        }

    }

}