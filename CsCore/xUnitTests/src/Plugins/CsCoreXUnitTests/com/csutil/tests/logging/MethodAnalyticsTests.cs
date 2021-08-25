using com.csutil.analytics;
using System.Linq;
using Xunit;

namespace com.csutil.tests {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class MethodAnalyticsTests {


        public MethodAnalyticsTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {
            var methodAnalytics = new MethodAnalytics() { includeMethodArguments = true };
            StopwatchV2 t = Log.MethodEntered();
            MyMethod1(true);
            Log.MethodDone(t);
            string report = methodAnalytics.ToString();
            Assert.True(1500 < report.Count(), "report.Count: " + report.Count());
            Log.d(report);
        }

        [Fact]
        public void ExampleUsage2() {
            var methodAnalytics = new MethodAnalytics();
            var t = Log.MethodEntered();
            MyMethod4("1");
            MyMethod4("2");
            MyMethod4("3");
            MyMethod4("4");
            MyMethod3("5");
            Log.MethodDone(t);
            var report = methodAnalytics.ToString();
            Assert.True(500 < report.Count(), "report.Count: " + report.Count());
        }

        private void MyMethod1(bool v) {
            using (Log.MethodEnteredWith(v)) {
                MyMethod2(10);
                MyMethod3("abc");
                if (v) { MyMethod1(false); }
            }
        }

        private void MyMethod2(int i) {
            using (Log.MethodEnteredWith(i)) {
                MyMethod3("abc");
                MyMethod3("" + i);
            }
        }

        private string MyMethod3(string s) {
            using (Log.MethodEnteredWith(s)) {
                return s + "def";
            }
        }

        private string MyMethod4(string s) {
            Log.MethodEnteredWith(s);
            return s + "def";
        }

    }

}