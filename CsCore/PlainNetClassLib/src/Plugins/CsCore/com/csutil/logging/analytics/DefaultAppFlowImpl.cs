using com.csutil.keyvaluestore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace com.csutil.logging.analytics {

    /// <summary> 
    /// A producer consumer wrapper around a KeyValue store that automatically registeres for internet 
    /// connectivity to send the collected events to an external system like an Analytics tracking system
    /// </summary>
    public abstract class DefaultAppFlowImpl : IAppFlow, IHasInternetListener {

        public IKeyValueStore store;

        public DefaultAppFlowImpl(IKeyValueStore store = null) {
            this.store = store == null ? FileBasedKeyValueStore.New("AppFlowEvents") : store;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            InternetStateManager.AddListener(this);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task OnHasInternet(bool hasInet) {
            if (hasInet) {
                var keys = (await store.GetAllKeys()).ToList();
                foreach (var key in keys) {
                    try {
                        if (await SendEventToExternalSystem(await store.Get<AppFlowEvent>(key, null))) {
                            var wasRemoved = await store.Remove(key);
                            AssertV2.IsTrue(wasRemoved, "Could not remove key " + key);
                        }
                    } catch (Exception e) { Log.e(e); }
                }
            }
        }

        protected abstract Task<bool> SendEventToExternalSystem(AppFlowEvent appFlowEvent);

        public void TrackEvent(string category, string action, params object[] args) {
            var e = new AppFlowEvent() { cat = category, action = action, args = args };
            store.Set(e.time + "__" + category + ":" + action, e);
        }

    }

    public class AppFlowEvent {
        public long time { get; set; } = DateTime.UtcNow.ToUnixTimestampUtc();
        public string cat { get; set; }
        public string action { get; set; }
        public object[] args { get; set; }
    }

}
