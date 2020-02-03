using System;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http;
using com.csutil.keyvaluestore;

namespace com.csutil.logging.analytics {

    public class GoogleAnalyticsTracker : DefaultAppFlowImpl {

        /// <summary> https://developers.google.com/analytics/devguides/collection/protocol/v1/validating-hits </summary>
        public const string DEBUG_ENDPOINT = "http://www.google-analytics.com/debug/collect";

        public string url = "http://www.google-analytics.com/collect";
        public string appId;
        private string latestScreen;

        /// <summary> </summary>
        /// <param name="gaTrackingId"> Tracking ID / Property ID. Format: UA-XXXXX-Y </param>
        /// <param name="store"> Optional store to cache the events in </param>
        public GoogleAnalyticsTracker(string gaTrackingId, IKeyValueStore store = null) : base(store) {
            this.appId = gaTrackingId;
        }

        protected override async Task<bool> SendEventToExternalSystem(AppFlowEvent e) {
            ExtractScreenName(e);
            var result = await SendGaEvent(e).GetResult<string>();
            return true;
        }

        // https://cloud.google.com/appengine/docs/flexible/nodejs/integrating-with-analytics#server-side_analytics_collection
        public RestRequest SendGaEvent(AppFlowEvent e, string label = null, int value = -1) {
            return SendToGA(NewGaEvent(e, label, value));
        }

        private RestRequest SendToGA(object parameters) {
            return new Uri(url + "?" + RestRequestHelper.ToUriEncodedString(parameters)).SendGET();
        }

        // Example: https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide#event
        private GAEvent NewGaEvent(AppFlowEvent e, string label, int value) {
            var r = new GAEvent() { tid = appId, t = "event", ec = e.cat, ea = e.action, el = label, cd = latestScreen };
            if (value > 0) { r.ev = value; }
            return r;
        }

        // Checks if a new screen was entered to cache the current screen name
        private bool ExtractScreenName(AppFlowEvent appFlowEvent) {
            try {
                if (appFlowEvent.cat == EventConsts.catView) {
                    if (appFlowEvent.action.StartsWith(EventConsts.SHOW_VIEW)) {
                        latestScreen = "" + appFlowEvent.args.First();
                        return true;
                    }
                    if (appFlowEvent.action.StartsWith(EventConsts.SWITCH_BACK_TO_LAST_VIEW)) {
                        latestScreen = "" + appFlowEvent.args.First();
                        return true;
                    }
                    if (appFlowEvent.action.StartsWith(EventConsts.SWITCH_TO_NEXT_VIEW)) {
                        latestScreen = "" + appFlowEvent.args[1];
                        return true;
                    }
                }
            } catch (Exception e) { Log.e(e); }
            return false;
        }

        // Docu at https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
        // Examples at https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide#overview
        public class GAEvent {

            /// <summary> An id generated in a static context to be used for all events </summary>
            public static string SESSION_ID = Guid.NewGuid().ToString();
            /// <summary> Anonymous Client ID. Required if uid not specified. A UUID associated with particular user, device, or browser instance. </summary>
            public string cid = SESSION_ID;
            /// <summary> User ID. Required if cid not specified. It must not itself be PII (personally identifiable information). </summary>
            public string uid;

            /// <summary> The Protocol version. The current value is '1' </summary>
            public readonly string v = "1";

            /// <summary> The tracking ID / web property ID. The format is typically UA-XXXX-Y </summary>
            public string tid;

            /// <summary> When not null, the IP address of the sender will be anonymized. </summary>
            public string aip = "1";

            /// <summary> Data source, e.g. "web" or "app" or "crm" </summary>
            public string ds = "app";

            /// <summary> 
            /// Event hit type. Must be one of: 
            /// 'pageview', 'screenview', 'event', 'transaction', 'item', 'social', 'exception', 'timing' 
            /// </summary>
            public string t;
            /// <summary> Event category. Must not be empty. </summary>
            public string ec;
            /// <summary> Event action. Must not be empty. </summary>
            public string ea;
            /// <summary> Event label. </summary>
            public string el;
            /// <summary> Event value. Values must be non-negative.
            public int? ev;
            /// <summary> Screen name.
            public string cd;

            /// <summary> Screen resolution e.g. "1234x1234" </summary>
            public string screenResolution;

            /// <summary> Campain ID </summary>
            public string cn;

            /// <summary> User language, e.g. "en-us" </summary>
            public string ul;

            /// <summary> App ID, e.g. "com.company.app" </summary>
            public string aid;
            /// <summary> App version, e.g. "1.1.2" </summary>
            public string av;

            /// <summary> user timing category </summary>
            public string utc;
            /// <summary> user timing variable </summary>
            public string utv;
            /// <summary> user timing value. The value is in milliseconds </summary>
            public int? utt;

        }

    }

}