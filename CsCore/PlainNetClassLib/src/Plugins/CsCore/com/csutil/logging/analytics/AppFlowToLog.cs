namespace com.csutil.analytics {

    public class AppFlowToLog : IAppFlow {

        public void TrackEvent(string cat, string action, params object[] _) {
            Log.d($"{cat.ToUpperInvariant()} : {action}");
        }

    }

}