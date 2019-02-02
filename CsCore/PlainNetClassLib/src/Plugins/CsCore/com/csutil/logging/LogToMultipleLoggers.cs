using System;
using System.Collections.Generic;
using System.Linq;

namespace com.csutil.logging {

    /// <summary> Can be used to log to multiple loggers at once </summary>
    public class LogToMultipleLoggers : ILog {

        public List<ILog> loggers = new List<ILog>();

        public void LogDebug(string msg, params object[] args) {
            foreach (var logger in loggers) { logger.LogDebug(msg, args); }
        }

        public Exception LogError(string error, params object[] args) {
            return loggers.Map(l => l.LogError(error, args)).ToList().FirstOrDefault();
        }

        public Exception LogExeption(Exception e, params object[] args) {
            return loggers.Map(l => l.LogExeption(e, args)).ToList().FirstOrDefault();
        }

        public void LogWarning(string warning, params object[] args) {
            foreach (var logger in loggers) { logger.LogWarning(warning, args); }
        }

    }

}