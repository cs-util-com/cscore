using com.csutil.ui;
using ReuseScroller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.logging {

    public class LogConsole {

        public static void Log(string d) {
            IoC.inject.GetOrAddSingleton<LogConsoleUi>(null, InitToastsUi).AddLog(d);
        }

        private static LogConsoleUi InitToastsUi() {
            var targetCanvas = CanvasFinder.GetOrAddRootCanvas().gameObject;
            var toastContainer = targetCanvas.AddChild(ResourcesV2.LoadPrefab("Messages/LogConsoleUi1"));
            return toastContainer.GetComponentInChildren<LogConsoleUi>();
        }

    }

    public class LogConsoleUi : BaseController<LogEntry> {

        internal void AddLog(string d) {
            CellData.Add(new LogEntry() { message = d, color = Color.gray, icon = "" });
            ReloadData();
        }

    }

    public class LogEntry {
        public DateTime createdAt = DateTime.UtcNow;
        public string message;
        public Color color;
        public string icon;
    }

}
