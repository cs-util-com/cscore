using com.csutil.logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil.testing {

    public class LogForXunitTestRunnerInUnity : LogToUnityDebugLog {

        public List<KeyValuePair<string, object[]>> collectedErrors = new List<KeyValuePair<string, object[]>>();

        protected override void PrintErrorMessage(string errorMsg, params object[] args) {
            collectedErrors.Add(new KeyValuePair<string, object[]>(errorMsg, args));
        }

        protected override void PrintException(Exception e, object[] args) {
            collectedErrors.Add(new KeyValuePair<string, object[]>("" + e, args));
        }

    }

}
