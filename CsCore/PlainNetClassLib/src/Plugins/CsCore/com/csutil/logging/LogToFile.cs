using System.IO;

namespace com.csutil.logging {

    public class LogToFile : LogDefaultImpl {

        private FileInfo targetFile;

        public LogToFile(FileInfo targetFileToLogInto) { targetFile = targetFileToLogInto; }

        protected override void PrintDebugMessage(string debugLogMsg, params object[] args) {
            throw new System.NotImplementedException();
        }

        protected override void PrintErrorMessage(string errorMsg, params object[] args) {
            throw new System.NotImplementedException();
        }

        protected override void PrintWarningMessage(string warningMsg, params object[] args) {
            throw new System.NotImplementedException();
        }

        protected override string ToString(object arg) {
            throw new System.NotImplementedException();
        }

    }

}

