using com.csutil.analytics;

namespace com.csutil {

    public interface IAppFlow {
        void TrackEvent(string category, string action, params object[] args);
    }

    public class AppFlow {

        public static IAppFlow instance;

        public static void TrackEvent(string category, string action, params object[] args) {
            instance?.TrackEvent(category, action, args);
        }

        public static void AddAppFlowTracker(IAppFlow tracker) {
            if (instance == null) {
                instance = tracker;
            } else if (instance is AppFlowMultipleTrackers multi) {
                multi.trackers.Add(tracker);
            } else {
                instance = new AppFlowMultipleTrackers(instance, tracker);
            }
        }

    }

}