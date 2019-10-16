namespace com.csutil {

    public interface IAppFlow {
        void TrackEvent(string category, string action, object[] args);
    }

    public class AppFlow {

        public const string catMethod = "method";
        public const string catError = "error";
        public const string catUi = "ui";
        public const string catScreen = "screen";
        public const string catMutation = "mutation";

        public static IAppFlow instance;

        public static void TrackEvent(string category, string action, object[] args) {
            instance?.TrackEvent(category, action, args);
        }

    }

}