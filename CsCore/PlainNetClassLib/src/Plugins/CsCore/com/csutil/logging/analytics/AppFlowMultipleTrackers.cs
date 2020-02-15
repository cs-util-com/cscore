using System.Collections.Generic;

namespace com.csutil.analytics {

    public class AppFlowMultipleTrackers : IAppFlow {

        public List<IAppFlow> trackers = new List<IAppFlow>();

        public AppFlowMultipleTrackers(params IAppFlow[] t) { this.trackers.AddRange(t); }

        public void TrackEvent(string category, string action, params object[] args) {
            foreach (var t in trackers) { t.TrackEvent(category, action, args); }
        }

    }

}