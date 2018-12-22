using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil.logging.tests {

    class TestLogging {

        [Test]
        public void TestSingleton() {
            Log.instance = new LogViaUnityDebugLog();
            LoggingExamples.RunLogExamples();
        }

    }

}
