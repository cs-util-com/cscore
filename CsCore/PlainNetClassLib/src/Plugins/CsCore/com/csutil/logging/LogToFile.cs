using System;
using System.Collections.Generic;
using System.IO;

namespace com.csutil.logging {

    public class LogToFile : LogDefaultImpl, IDisposable {

        private const string JSON_LB = LB + ",";
        private FileStream s;
        private TextWriter w;
        private IJsonWriter jsonWriter;

        public LogToFile(FileInfo targetFileToLogInto) {
            s = new FileStream(targetFileToLogInto.FullPath(), FileMode.Append, FileAccess.Write, FileShare.Read);
            w = TextWriter.Synchronized(new StreamWriter(s));
            jsonWriter = JsonWriter.GetWriter();
        }

        protected override void PrintDebugMessage(string debugLogMsg, params object[] args) {
            var asJson = jsonWriter.Write(new LogEntry() { d = debugLogMsg });
            w.WriteLine(asJson + JSON_LB);
            w.Flush();
        }

        protected override void PrintWarningMessage(string warningMsg, params object[] args) {
            var asJson = jsonWriter.Write(new LogEntry() { w = warningMsg });
            w.WriteLine(asJson + JSON_LB);
            w.Flush();
        }

        protected override void PrintErrorMessage(string errorMsg, params object[] args) {
            var asJson = jsonWriter.Write(new LogEntry() { e = errorMsg });
            w.WriteLine(asJson + JSON_LB);
            w.Flush();
        }

        public void Dispose() {
            w.Dispose();
            s.Dispose();
        }

        public static LogToFile.LogStructure LoadLogFile(System.IO.FileInfo targetFileToLogInto) {
            var logFileContent = targetFileToLogInto.LoadAs<string>();
            logFileContent = "{\"logEntries\":[" + logFileContent + "]}";
            var logStructure = JsonReader.GetReader().Read<LogToFile.LogStructure>(logFileContent);
            return logStructure;
        }

        public class LogStructure {
            public List<LogEntry> logEntries;
        }

        public class LogEntry {
            public string d;
            public string w;
            public string e;
        }

    }

}

