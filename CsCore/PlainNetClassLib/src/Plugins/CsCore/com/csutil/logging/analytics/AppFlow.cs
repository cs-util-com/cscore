namespace com.csutil {

    public interface IAppFlow {
        void TrackEvent(string category, string action, params object[] args);
    }

    public class AppFlow {

        public static IAppFlow instance;

        public static void TrackEvent(string category, string action, params object[] args) {
            instance?.TrackEvent(category, action, args);
        }

    }

}