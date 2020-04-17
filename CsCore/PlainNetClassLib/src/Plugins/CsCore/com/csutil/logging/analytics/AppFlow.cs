using com.csutil.analytics;

namespace com.csutil {

    public interface IAppFlow {
        void TrackEvent(string category, string action, params object[] args);
    }

    public static class AppFlow {

        public static IAppFlow instance(object caller) { return IoC.inject.Get<IAppFlow>(caller); }

        public static void TrackEvent(string category, string action, params object[] args) {
            instance(category)?.TrackEvent(category, action, args);
        }

        public static void AddAppFlowTracker(IAppFlow tracker) {
            var oldInstance = instance(null);
            if (oldInstance == null) {
                IoC.inject.SetSingleton(tracker);
            } else if (oldInstance is AppFlowMultipleTrackers multi) {
                multi.trackers.Add(tracker);
            } else {
                IoC.inject.SetSingleton(new AppFlowMultipleTrackers(oldInstance, tracker));
            }
        }

    }

}