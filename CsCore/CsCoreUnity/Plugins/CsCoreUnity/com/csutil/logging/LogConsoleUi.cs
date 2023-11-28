using com.csutil.ui;
using ReuseScroller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.logging {

    public static class LogConsole {

        public static void RegisterForAllLogEvents(object caller, bool hideAtStart = true) {
            var consoleUi = GetLogConsole(caller);
            var logger = new LogToLogConsoleConnector(consoleUi);
            logger.RegisterForLoggingV2_AllLogEvents();
            consoleUi.ShowConsole(!hideAtStart);
        }

        /// <summary> This will only log the errors send to the csutil Log system </summary>
        public static void RegisterForLoggingV1_OnlyExplicitLogCalls(this LogToLogConsoleConnector logger) {
            Log.AddLoggerToLogInstances(logger);
            logger.logUi.gameObject.AddOnDestroyListener(() => {
                Log.RemoveLoggerFromLogInstances(logger);
            });
        }

        /// <summary> This will log all errors send to any logging sink </summary>
        public static void RegisterForLoggingV2_AllLogEvents(this LogToLogConsoleConnector logger) {
            Application.logMessageReceivedThreaded += logger.HandleLogMessageReceivedThreaded;
            logger.logUi.gameObject.AddOnDestroyListener(() => {
                Application.logMessageReceivedThreaded -= logger.HandleLogMessageReceivedThreaded;
            });
        }

        public static LogConsoleUi GetLogConsole(object caller) {
            return IoC.inject.GetOrAddSingleton<LogConsoleUi>(caller, InitLogConsoleUi);
        }

        private static LogConsoleUi InitLogConsoleUi() {
            var targetCanvas = RootCanvas.GetOrAddRootCanvasV2().gameObject;
            var toastContainer = targetCanvas.AddChild(ResourcesV2.LoadPrefab("Messages/LogConsoleUi1"));
            return toastContainer.GetComponentInChildren<LogConsoleUi>();
        }

    }

    public class LogToLogConsoleConnector : LogDefaultImpl {

        internal LogConsoleUi logUi;

        public LogToLogConsoleConnector(LogConsoleUi logConsoleUi) { this.logUi = logConsoleUi; }

        protected override void PrintDebugMessage(string d, params object[] args) { logUi.AddToLog(LogEntry.d(d)); }
        protected override void PrintInfoMessage(string i, params object[] args) { logUi.AddToLog(LogEntry.i(i)); }
        protected override void PrintWarningMessage(string w, params object[] args) { logUi.AddToLog(LogEntry.w(w)); }
        protected override void PrintErrorMessage(string e, params object[] args) { logUi.AddToLog(LogEntry.e(e)); }

        public override StopwatchV2 LogMethodEntered(string methodName, object[] args) {
            logUi.AddToLog(LogEntry.d(" --> " + methodName + " args=" + args.ToStringV2(x => "" + x)));
            return null;
        }

        public override void LogMethodDone(Stopwatch timing, object[] args, int maxAllowedTimeInMs, string sourceMemberName, string sourceFilePath, int sourceLineNumber) {
            string methodName = sourceMemberName;
            if (timing is StopwatchV2 t2) { methodName = t2.methodName; }
            var argStr = args != null ? " with " + args.Filter(a => a != null).Map(a => "" + a).ToStringV2() : "";
            logUi.AddToLog(LogEntry.d("    <-- " + methodName + " finished after " + timing.ElapsedMilliseconds + " ms" + argStr));
        }

        private const string LINE_BREAK = "\n";

        public void HandleLogMessageReceivedThreaded(string condition, string stacktrace, LogType type) {
            TaskV2.TryWithExponentialBackoff(async () => {
                if (!ApplicationV2.isPlaying) { return; }
                // Wait for the main thread to be ready to use:
                while (!MainThread.IsReadyToUse) { await TaskV2.Delay(5); }
                MainThread.Invoke(() => {
                    switch (type) {
                        case LogType.Log:
                            PrintDebugMessage(condition + LINE_BREAK + stacktrace);
                            break;
                        case LogType.Warning:
                            PrintWarningMessage(condition + LINE_BREAK + stacktrace);
                            break;
                        case LogType.Assert:
                        case LogType.Error:
                        case LogType.Exception:
                            PrintErrorMessage(condition + LINE_BREAK + stacktrace);
                            break;
                    }
                });
            }, maxNrOfRetries: 1, initialExponent: 9).LogOnError();
        }

    }

    public class LogConsoleUi : BaseController<LogEntry> {

        private List<LogEntry> allData = new List<LogEntry>();
        private Func<LogEntry, bool> filter = (x) => true;

        private Dictionary<string, Link> map;
        private InputField SearchInputField() { return map.Get<InputField>("Search"); }
        private Toggle ToggleShowDebugs() { return map.Get<Toggle>("ToggleShowDebugs"); }
        private Toggle ToggleShowInfos() { return map.Get<Toggle>("ToggleShowInfos"); }
        private Toggle ToggleShowWarngs() { return map.Get<Toggle>("ToggleShowWarnings"); }
        private Toggle ToggleShowErrors() { return map.Get<Toggle>("ToggleShowErrors"); }

        protected override void OnEnable() {
            base.OnEnable();
            InitMap();
            map.Get<Button>("BtnClear").SetOnClickAction(delegate { ClearConsole(); });
            map.Get<Button>("BtnHideLog").SetOnClickAction(delegate { ToggleConsoleVisibility(); });
            ToggleShowDebugs().SetOnValueChangedAction(delegate {
                UpdateFilter(NewFilter());
                return true;
            });
            ToggleShowInfos().SetOnValueChangedAction(delegate {
                UpdateFilter(NewFilter());
                return true;
            });
            ToggleShowWarngs().SetOnValueChangedAction(delegate {
                UpdateFilter(NewFilter());
                return true;
            });
            ToggleShowErrors().SetOnValueChangedAction(delegate {
                UpdateFilter(NewFilter());
                return true;
            });
            SearchInputField().SetOnValueChangedAction(delegate {
                UpdateFilter(NewFilter());
                return true;
            });
        }

        private void InitMap() {
            var c = gameObject.GetComponentInParents<Canvas>();
            AssertV2.IsNotNull(c, "Canvas in LogConsoleUi parent");
            if (c == null) { c = gameObject.GetParent().GetComponentV2<Canvas>(); }
            map = c.gameObject.GetLinkMap();
        }

        private void ToggleConsoleVisibility() { ShowConsole(!gameObject.activeSelf); }

        public void ShowConsole(bool isConsoleVisible) {
            gameObject.SetActiveV2(isConsoleVisible);
            InitMap();
            map.Get<CanvasGroup>("MenuButtons").interactable = isConsoleVisible;
            map.Get<CanvasGroup>("MenuButtons").blocksRaycasts = isConsoleVisible;
        }

        public void ClearConsole() {
            allData.Clear();
            CellData.Clear();
            ReloadData();
        }

        private Func<LogEntry, bool> NewFilter() {
            bool d = ToggleShowDebugs().isOn;
            bool i = ToggleShowInfos().isOn;
            bool w = ToggleShowWarngs().isOn;
            bool e = ToggleShowErrors().isOn;
            string s = SearchInputField().text.ToLowerInvariant();
            return (logEntry) => {
                if (!d && logEntry.type == "d") { return false; }
                if (!i && logEntry.type == "i") { return false; }
                if (!w && logEntry.type == "w") { return false; }
                if (!e && logEntry.type == "e") { return false; }
                if (!s.IsNullOrEmpty() && !logEntry.message.ToLowerInvariant().Contains(s)) { return false; }
                return true;
            };
        }

        private void UpdateFilter(Func<LogEntry, bool> newFilter) {
            filter = newFilter;
            CellData = allData.Filter(filter).ToList();
            ReloadData();
        }

        public void AddToLog(LogEntry logMessage) {
            if (!ApplicationV2.isPlaying) { return; }
            allData.Add(logMessage);
            if (filter(logMessage)) {
                CellData.Add(logMessage);
                ReloadData();
            }
        }

    }

    public class LogEntry {

        public const string ICON_D = "";
        public const string ICON_I = "";
        public const string ICON_W = "";
        public const string ICON_E = "";

        public static Color COLOR_GRAY = Color.black;
        public static Color COLOR_WHITE = Color.white;
        public static Color COLOR_YELLOW = Color.yellow;
        public static Color COLOR_RED = Color.red;

        public DateTime createdAt = DateTimeV2.UtcNow;
        public string message;
        public Color color;
        public string icon;
        public string type;

        public static LogEntry d(string d) { return new LogEntry() { message = d, color = COLOR_GRAY, icon = ICON_D, type = "d" }; }
        public static LogEntry i(string i) { return new LogEntry() { message = i, color = COLOR_WHITE, icon = ICON_I, type = "i" }; }
        public static LogEntry w(string w) { return new LogEntry() { message = w, color = COLOR_YELLOW, icon = ICON_W, type = "w" }; }
        public static LogEntry e(string e) { return new LogEntry() { message = e, color = COLOR_RED, icon = ICON_E, type = "e" }; }

    }

}