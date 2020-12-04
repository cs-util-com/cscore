using ReuseScroller;
using UnityEngine.UI;

namespace com.csutil.logging.ui {

    public class LogConsoleEntryUi : BaseCell<LogEntry> {

        public Text logText;
        public Image background;
        public Text icon;

        public override void UpdateContent(LogEntry item) {
            logText.text = item.createdAt.ToString("[HH:mm:ss] ") + item.message;
            icon.text = item.icon;
            background.color = item.color;
            var compl = item.color.GetContrastBlackOrWhite();
            logText.color = compl;
            icon.color = compl;
        }

    }

}
