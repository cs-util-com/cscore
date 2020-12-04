using com.csutil.logging;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace com.csutil.tests {

    /// <summary> The most advanced AI for automatic error resolution ever ;) </summary>
    public class Ui25_LazyDeveloperAI : UnitTestMono {

        public override IEnumerator RunTest() { yield return ShowUI().AsCoroutine(); }

        private async Task ShowUI() {

            // Register the logger that looks up solutions for errors automatically:
            Log.AddLoggerToLogInstances(new LazyDeveloperLogger());

            var map = gameObject.GetLinkMap();

            // Each button causes a different type of error:
            var b1 = map.Get<Button>("Button1").SetOnClickAction(delegate {
                string[] a = new string[0] { };
                a.First();
            });
            var b2 = map.Get<Button>("Button2").SetOnClickAction(delegate {
                string[] a = new string[1] { "1" };
                a[1] = "oops";
            });
            var b3 = map.Get<Button>("Button3").SetOnClickAction(delegate {
                CauseStackOverflowException();
            });
            var b4 = map.Get<Button>("Button4").SetOnClickAction(delegate {
                object o = "1";
                var i = (int)o;
            });
            var b5 = map.Get<Button>("Button5").SetOnClickAction(delegate {
                MemoryStream m = new MemoryStream();
                m.Dispose();
                m.ReadByte();
            });
            var b6 = map.Get<Button>("Button6").SetOnClickAction(delegate {
                new List<string>().RemoveAt(0);
            });
            var b7 = map.Get<Button>("Button7").SetOnClickAction(delegate {
                var a = new string[1] { "1" };
                a.SetValue(value: 1, index: 0);
            });
            var b8 = map.Get<Button>("Button8").SetOnClickAction(delegate {
                var x = 0;
                var y = 10 / x;
            });

            // Lists of even more common exceptions to get inspired by for testing: 
            // https://www.developerfusion.com/article/1889/exception-handling-in-c/3/
            // https://www.completecsharptutorial.com/basic/complete-system-exception.php
            // https://mikevallotton.wordpress.com/2009/07/08/net-exceptions-all-of-them/

            // Finish test after all buttons pressed:
            await Task.WhenAll(b1, b2, b3, b4, b5, b6, b7, b8);
        }

        private void CauseStackOverflowException() {
            CauseStackOverflowException(); // Recursive call until infinity
        }

    }

}