using com.csutil.analytics;
using System.Collections.Generic;
using System.Linq;

namespace com.csutil {

    public interface IAppFlow {
        void TrackEvent(string category, string action, params object[] args);
    }

    public static class AppFlow {

        public static IAppFlow instance(object caller) { return IoC.inject.Get<IAppFlow>(caller); }

        public static void TrackEvent(string category, string action, params object[] args) {
            instance(category)?.TrackEvent(category, action, args);
        }

        public static AppFlowMultipleTrackers AddAppFlowTracker(IAppFlow tracker) {
            var oldInstance = instance(null);
            if (oldInstance == null) {
                var m1 = new AppFlowMultipleTrackers(tracker);
                IoC.inject.SetSingleton<IAppFlow>(m1);
                return m1;
            }
            if (oldInstance is AppFlowMultipleTrackers m2) {
                m2.trackers.Add(tracker);
                return m2;
            }
            var m3 = new AppFlowMultipleTrackers(oldInstance, tracker);
            IoC.inject.SetSingleton<IAppFlow>(m3);
            return m3;
        }

        public static IEnumerable<T> GetAllOfType<T>() where T : IAppFlow {
            IAppFlow i = AppFlow.instance(null);
            if (i is AppFlowMultipleTrackers m) { return m.trackers.OfType<T>(); }
            if (i is T) { return new T[1] { (T)i }; }
            return null;
        }

    }

}