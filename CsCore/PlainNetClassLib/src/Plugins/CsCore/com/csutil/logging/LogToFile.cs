using System;
using System.Collections.Generic;
using System.IO;

namespace com.csutil.logging {

    public class LogToFile : LogDefaultImpl, IDisposable {

        private const string JSON_LB = LB + ",";
        public FileInfo logFile;
        private FileStream fileStream;
        private TextWriter writer;
        private IJsonWriter jsonWriter;

        public LogToFile(FileInfo targetFileToLogInto) {
            this.logFile=targetFileToLogInto;
            fileStream = new FileStream(targetFileToLogInto.FullPath(), FileMode.Append, FileAccess.Write, FileShare.Read);
            writer = TextWriter.Synchronized(new StreamWriter(fileStream));
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
            fileStream.Dispose();
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

