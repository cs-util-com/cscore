using System;
using System.Diagnostics;

namespace com.csutil.logging {
    public interface ILog {
        void log(string msg, params object[] args);
        void logWarning(string warning, params object[] args);
        Exception logError(string error, params object[] args);
        Exception logExeption(Exception e, params object[] args);
    }
}