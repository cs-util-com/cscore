using System.Threading.Tasks;
using com.csutil.keyvaluestore;

namespace com.csutil.logging.analytics {

    public class AppFlowToStore : IAppFlow, IDisposableV2 {

        protected const string DEFAULT_FOLDER = "AppFlowEvents";
        public IKeyValueStoreTypeAdapter<AppFlowEvent> store;

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;
        
        public AppFlowToStore(IKeyValueStoreTypeAdapter<AppFlowEvent> store = null) {
            if (store == null) {
                store = FileBasedKeyValueStore.New(DEFAULT_FOLDER).GetTypeAdapter<AppFlowEvent>();
            }
            this.store = store;
        }

        public virtual void Dispose() {
            IsDisposed = DisposeState.DisposingStarted;
            store.DisposeV2();
            IsDisposed = DisposeState.Disposed;
        }

        public void TrackEvent(string category, string action, params object[] args) {
            this.ThrowErrorIfDisposed();
            var e = new AppFlowEvent() { cat = category, action = action, args = args };
            store.Set(e.time + "__" + category + "-" + action + ".json", e).LogOnError();
        }

    }

}
