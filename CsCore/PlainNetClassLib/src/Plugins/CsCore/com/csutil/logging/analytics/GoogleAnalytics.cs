using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http;
using com.csutil.keyvaluestore;

namespace com.csutil.logging.analytics {

    public class GoogleAnalytics : DefaultAppFlowImpl {

        /// <summary> https://developers.google.com/analytics/devguides/collection/protocol/v1/validating-hits </summary>
        public const string DEBUG_ENDPOINT = "https://www.google-analytics.com/debug/collect";

        public string url = "https://www.google-analytics.com/collect";
        public string appId;
        public string appName;
        private string latestScreen;

        /// <summary> Creates a new Google analytics tracker </summary>
        /// <param name="gaTrackingId"> Tracking ID / Property ID. Format: UA-XXXXX-Y </param>
        /// <param name="store"> Optional store to cache the events in </param>
        public GoogleAnalytics(string gaTrackingId, string appName, KeyValueStoreTypeAdapter<AppFlowEvent> store = null) : base(store) {
            this.appId = gaTrackingId;
            this.appName = appName;
        }

        protected override async Task<bool> SendEventToExternalSystem(AppFlowEvent e) {
            this.ThrowErrorIfDisposed();
            ExtractScreenName(e);
            if (e.args?.FirstOrDefault(x => x is Stopwatch) is Stopwatch timing) {
                await SendToGA(NewTiming(e.cat, e.action, timingInMs: timing.ElapsedMilliseconds)).GetResult<string>();
            } else {
                await SendToGA(NewEvent(e.cat, e.action)).GetResult<string>();
            }
            return true;
        }

        // https://cloud.google.com/appengine/docs/flexible/nodejs/integrating-with-analytics#server-side_analytics_collection
        public RestRequest SendToGA(BaseMsg parameters) {
            this.ThrowErrorIfDisposed();
            return new Uri(url + "?" + RestRequestHelper.ToUriEncodedString(parameters)).SendGET();
        }

        public Timing NewTiming(string category, string varName, long timingInMs) {
            this.ThrowErrorIfDisposed();
            return new Timing() { tid = appId, an = appName, cd = latestScreen, utc = category, utv = varName, utt = timingInMs };
        }

        // Example: https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide#event
        public Event NewEvent(string category, string action, string label = null, int value = -1) {
            this.ThrowErrorIfDisposed();
            var r = new Event() { tid = appId, an = appName, ec = category, ea = action, el = label, cd = latestScreen };
            if (value > 0) { r.ev = value; }
            return r;
        }

        // Checks if a new screen was entered to cache the current screen name
        private bool ExtractScreenName(AppFlowEvent appFlowEvent) {
            try {
                if (appFlowEvent.cat == EventConsts.catView) {
                    if (appFlowEvent.action.StartsWith(EventConsts.SHOW)) {
                        latestScreen = "" + appFlowEvent.args.First();
                        return true;
                    }
                    if (appFlowEvent.action.StartsWith(EventConsts.SWITCH_BACK_TO_LAST)) {
                        latestScreen = "" + appFlowEvent.args.First();
                        return true;
                    }
                    if (appFlowEvent.action.StartsWith(EventConsts.SWITCH_TO_NEXT)) {
                        latestScreen = "" + appFlowEvent.args[1];
                        return true;
                    }
                    if (appFlowEvent.action.StartsWith(EventConsts.ADDED)) {
                        latestScreen = "" + appFlowEvent.args.First();
                        return true;
                    }
                }
            } catch (Exception e) { Log.e(e); }
            return false;
        }

        // Docu at https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
        // Examples at https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide#overview
        public class BaseMsg {

            public BaseMsg(string type) { t = type; }

            // Event hit type. Must be one of: 
            // 'pageview', 'screenview', 'event', 'transaction', 'item', 'social', 'exception', 'timing' 
            public readonly string t = "event";

            /// <summary> An id generated in a static context to be used for all events </summary>
            public static string SESSION_ID = GuidV2.NewGuid().ToString();
            /// <summary> Anonymous Client ID. Required if uid not specified. A UUID associated with particular user, device, or browser instance. </summary>
            public string cid = SESSION_ID;
            /// <summary> User ID. Required if cid not specified. It must not itself be PII (personally identifiable information). </summary>
            public string uid;

            /// <summary> The Protocol version. The current value is '1' </summary>
            public readonly string v = "1";

            /// <summary> The tracking ID / web property ID. The format is typically UA-XXXX-Y </summary>
            public string tid;

            /// <summary> The tracking ID / web property ID. The format is typically UA-XXXX-Y </summary>
            public string an;

            /// <summary> When not null, the IP address of the sender will be anonymized. </summary>
            public string aip = "1";

            /// <summary> Data source, e.g. "web" or "app" or "crm" </summary>
            public string ds = "app";

            /// <summary> Screen resolution e.g. "1280x800" </summary>
            public string sr;

            /// <summary> Campain ID </summary>
            public string cn;

            /// <summary> User language, e.g. "en-us" </summary>
            public string ul;

            /// <summary> App ID, e.g. "com.company.app" </summary>
            public string aid;
            /// <summary> App version, e.g. "1.1.2" </summary>
            public string av;
            /// <summary> Screen name.
            public string cd;
        }

        public class Event : BaseMsg {

            public Event() : base("event") { }

            /// <summary> Event category. Must not be empty. </summary>
            public string ec;
            /// <summary> Event action. Must not be empty. </summary>
            public string ea;
            /// <summary> Event label. </summary>
            public string el;
            /// <summary> Event value. Values must be non-negative.
            public int? ev;

        }

        public class Timing : BaseMsg {

            public Timing() : base("timing") { }

            /// <summary> user timing category </summary>
            public string utc;
            /// <summary> user timing variable </summary>
            public string utv;
            /// <summary> user timing value. The value is in milliseconds </summary>
            public long? utt;

        }

    }

}