using System;
using System.Diagnostics;

namespace com.csutil.logging {

    public interface ILog {

        void LogDebug(string msg, params object[] args);
        void LogWarning(string warning, params object[] args);
        Exception LogError(string error, params object[] args);
        Exception LogExeption(Exception e, params object[] args);
        StopwatchV2 LogMethodEntered(string methodName, object[] args);
        void LogMethodDone(Stopwatch timing, object[] args, int maxAllowedTimeInMs, string sourceMemberName, string sourceFilePath, int sourceLineNumber);

    }

}