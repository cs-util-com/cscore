using com.csutil.http.apis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.csutil.logging {

    /// <summary> This logger will take every error and look it up on 
    /// StackOverflow.com because it can.. </summary>
    public class LazyDeveloperLogger : LogDefaultImpl {

        private Dictionary<string, string> cache = new Dictionary<string, string>();
        public List<string> tags = new List<string>() { "C#" };
        private string lastKey;

        protected override void PrintErrorMessage(string errorMsg, params object[] args) {
            if (!EnvironmentV2.isDebugMode) { return; }
            var exception = args.OfType<Exception>().FirstOrDefault();
            if (exception != null && !exception.Message.IsNullOrEmpty()) {
                errorMsg = exception.Message;
            }
            CheckStackOverflowForError(errorMsg).LogOnError();
        }

        private async Task CheckStackOverflowForError(string errorMsg) {
            try {
                var question = StackOverflowCom.ExtractRelevantWords(errorMsg);
                if (lastKey == question) { return; } // Cancel if new error is same as last one 
                lastKey = question;
                if (!cache.ContainsKey(question)) {
                    var summary = await StackOverflowCom.Ask(question, tags, maxResults: 2);
                    cache.Add(question, summary);
                    Log.w("<color=#89FF84> >> Found a possible fix for error:</color>" + summary);
                }
            }
            catch (Exception e) { Log.d("" + e); }
        }

        protected override void PrintDebugMessage(string debugLogMsg, params object[] args) { }
        protected override void PrintInfoMessage(string infoLogMsg, params object[] args) { }
        protected override void PrintWarningMessage(string warningMsg, params object[] args) { }

    }

}