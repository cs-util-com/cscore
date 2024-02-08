using System;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;

namespace com.csutil.logging.analytics {

    /// <summary> 
    /// A producer consumer wrapper around a KeyValue store that automatically registeres for internet 
    /// connectivity to send the collected events to an external system like an Analytics tracking system
    /// </summary>
    public abstract class DefaultAppFlowImpl : AppFlowToStore, IHasInternetListener {
        private bool? oldHasInet;

        public DefaultAppFlowImpl(KeyValueStoreTypeAdapter<AppFlowEvent> store = null) : base(store) {
            InternetStateManager.AddListener(this);
        }

        public override void Dispose() {
            base.Dispose();
            InternetStateManager.RemoveListener(this);
        }

        public async Task OnHasInternet(bool hasInet) {
            this.ThrowErrorIfDisposed();
            if (oldHasInet != hasInet) {
                if (oldHasInet != null) {
                    EventBus.instance.Publish(EventConsts.catSystem + EventConsts.INET_CHANGED, oldHasInet, hasInet);
                }
                oldHasInet = hasInet;
            }
            if (hasInet) {
                foreach (var key in (await store.GetAllKeys()).ToList()) {
                    try {
                        if (await SendEventToExternalSystem(await store.Get(key, null))) {
                            var wasRemoved = await store.Remove(key);
                            AssertV3.IsTrue(wasRemoved, () => "Could not remove key " + key);
                        }
                    }
                    catch (Exception e) { Log.e(e); }
                }
            }
        }

        protected abstract Task<bool> SendEventToExternalSystem(AppFlowEvent appFlowEvent);

    }

}
