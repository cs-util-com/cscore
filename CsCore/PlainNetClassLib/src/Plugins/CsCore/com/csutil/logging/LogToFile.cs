using System;
using System.Collections.Generic;
using System.IO;
using Zio;

namespace com.csutil.logging {

    public class LogToFile : LogDefaultImpl, IDisposable {

        private const string JSON_LB = LB + ",";
        public FileEntry logFile;
        private Stream stream;
        private TextWriter writer;

        public LogToFile(FileEntry targetFileToLogInto) {
            this.logFile = targetFileToLogInto;
            stream = targetFileToLogInto.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
            writer = TextWriter.Synchronized(new StreamWriter(stream));
        }

        protected override void PrintDebugMessage(string debugLogMsg, params object[] args) {
            LogEntry logEntry = new LogEntry() { d = debugLogMsg };
            var asJson = JsonWriter.GetWriter(logEntry).Write(logEntry);
            writer.WriteLine(asJson + JSON_LB);
            writer.Flush();
        }

        protected override void PrintInfoMessage(string infoLogMsg, params object[] args) {
            LogEntry logEntry = new LogEntry() { i = infoLogMsg };
            var asJson = JsonWriter.GetWriter(logEntry).Write(logEntry);
            writer.WriteLine(asJson + JSON_LB);
            writer.Flush();
        }

        protected override void PrintWarningMessage(string warningMsg, params object[] args) {
            LogEntry logEntry = new LogEntry() { w = warningMsg };
            var asJson = JsonWriter.GetWriter(logEntry).Write(logEntry);
            writer.WriteLine(asJson + JSON_LB);
            writer.Flush();
        }

        protected override void PrintErrorMessage(string errorMsg, params object[] args) {
            LogEntry logEntry = new LogEntry() { e = errorMsg };
            var asJson = JsonWriter.GetWriter(logEntry).Write(logEntry);
            writer.WriteLine(asJson + JSON_LB);
            writer.Flush();
        }

        public void Dispose() {
            writer.Dispose();
            stream.Dispose();
        }

        public static LogStructure LoadLogFile(FileEntry targetFileToLogInto) {
            var logFileContent = targetFileToLogInto.LoadAs<string>(null, FileShare.ReadWrite);
            logFileContent = "{\"logEntries\":[" + logFileContent + "]}";
            return JsonReader.GetReader().Read<LogStructure>(logFileContent);
        }

        public class LogStructure {
            public List<LogEntry> logEntries;
        }

        public class LogEntry {
            public string d;
            public string i;
            public string w;
            public string e;
        }

    }

}