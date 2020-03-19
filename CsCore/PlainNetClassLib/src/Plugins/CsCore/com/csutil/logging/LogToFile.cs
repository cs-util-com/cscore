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
        private IJsonWriter jsonWriter;

        public LogToFile(FileEntry targetFileToLogInto) {
            this.logFile = targetFileToLogInto;
            stream = targetFileToLogInto.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
            writer = TextWriter.Synchronized(new StreamWriter(stream));
            jsonWriter = JsonWriter.GetWriter();
        }

        protected override void PrintDebugMessage(string debugLogMsg, params object[] args) {
            var asJson = jsonWriter.Write(new LogEntry() { d = debugLogMsg });
            writer.WriteLine(asJson + JSON_LB);
            writer.Flush();
        }

        protected override void PrintWarningMessage(string warningMsg, params object[] args) {
            var asJson = jsonWriter.Write(new LogEntry() { w = warningMsg });
            writer.WriteLine(asJson + JSON_LB);
            writer.Flush();
        }

        protected override void PrintErrorMessage(string errorMsg, params object[] args) {
            var asJson = jsonWriter.Write(new LogEntry() { e = errorMsg });
            writer.WriteLine(asJson + JSON_LB);
            writer.Flush();
        }

        public void Dispose() {
            writer.Dispose();
            stream.Dispose();
        }

        public static LogStructure LoadLogFile(FileEntry targetFileToLogInto) {
            var logFileContent = targetFileToLogInto.LoadAs<string>(FileShare.ReadWrite);
            logFileContent = "{\"logEntries\":[" + logFileContent + "]}";
            return JsonReader.GetReader().Read<LogStructure>(logFileContent);
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

