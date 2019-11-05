using com.csutil.ui;
using ReuseScroller;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.logging {

    public class LogConsole {

        public static void RegisterForAllLogEvents(object caller) {
            Log.AddLoggerToLogInstances(new LogToLogConsoleConnector(GetLogConsole(caller)));
        }

        public static LogConsoleUi GetLogConsole(object caller) {
            return IoC.inject.GetOrAddSingleton<LogConsoleUi>(caller, InitLogConsoleUi);
        }

        private static LogConsoleUi InitLogConsoleUi() {
            var targetCanvas = CanvasFinder.GetOrAddRootCanvas().gameObject;
            var toastContainer = targetCanvas.AddChild(ResourcesV2.LoadPrefab("Messages/LogConsoleUi1"));
            return toastContainer.GetComponentInChildren<LogConsoleUi>();
        }

    }

    internal class LogToLogConsoleConnector : LogDefaultImpl {

        private LogConsoleUi logUi;
        public LogToLogConsoleConnector(LogConsoleUi logConsoleUi) { this.logUi = logConsoleUi; }

        protected override void PrintDebugMessage(string d, params object[] args) { logUi.AddToLog(LogEntry.d(d)); }
        protected override void PrintWarningMessage(string w, params object[] args) { logUi.AddToLog(LogEntry.w(w)); }
        protected override void PrintErrorMessage(string e, params object[] args) { logUi.AddToLog(LogEntry.e(e)); }

    }

    public class LogConsoleUi : BaseController<LogEntry> {

        protected override void Start() {
            var map = gameObject.GetComponentInParents<Canvas>().gameObject.GetLinkMap();
            map.Get<Button>("BtnClear").SetOnClickAction(delegate {
                Log.d("BtnClear");
            });
            map.Get<Button>("BtnShowErrorsOnly").SetOnClickAction(delegate {
                Log.d("BtnShowErrorsOnly");
            });
            map.Get<Button>("BtnHideLog").SetOnClickAction(delegate {
                Log.d("BtnHideLog");
            });
        }

        public void AddToLog(LogEntry logMessage) { CellData.Add(logMessage); ReloadData(); }
    }

    public class LogEntry {

        public const string ICON_D = "";
        public const string ICON_W = "";
        public const string ICON_E = "";

        public static Color COLOR_GRAY = Color.gray;
        public static Color COLOR_YELLOW = Color.yellow;
        public static Color COLOR_RED = Color.red;

        public DateTime createdAt = DateTime.UtcNow;
        public string message;
        public Color color;
        public string icon;

        public static LogEntry d(string d) { return new LogEntry() { message = d, color = COLOR_GRAY, icon = ICON_D }; }
        public static LogEntry w(string w) { return new LogEntry() { message = w, color = COLOR_YELLOW, icon = ICON_W }; }
        public static LogEntry e(string e) { return new LogEntry() { message = e, color = COLOR_RED, icon = ICON_E }; }

    }

}
