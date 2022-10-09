using com.csutil.logging;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class Ui8_LogConsole : UnitTestMono {

        public override IEnumerator RunTest() {

            // Enable the log console and register to show all logging events in it:
            LogConsole.RegisterForAllLogEvents(this);

            // Configure a button to log errors when clicked:
            var map = gameObject.GetLinkMap();
            map.Get<Button>("Save").SetOnClickAction(delegate { Log.e("Save button clicked"); });

            // Print out a few example log entries manually:
            for (int i = 0; i < 10; i++) {
                LogConsole.GetLogConsole(this).AddToLog(LogEntry.d("Log event " + i));
                yield return new WaitForSeconds(0.5f);
            }

            // Entries can have custom colors, icons, ..:
            LogEntry entry = LogEntry.d("All logged");
            entry.color = Color.green.GetDarkerVariant();
            LogConsole.GetLogConsole(this).AddToLog(entry);

            TestIfThrownExceptionsAreShownInTheConsole().LogOnError();
            
            // Throw an exception that is not reported to the Log class but only detected by Unity logging:
            throw new System.Exception("I am an exception 1"); 

        }

        private async Task TestIfThrownExceptionsAreShownInTheConsole() {
            await TaskV2.Delay(500);
            throw new System.Exception("I am an exception 2");
        }

    }

}