using System.Threading.Tasks;
using com.csutil.keyvaluestore;

namespace com.csutil.logging.analytics {

    public class AppFlowToStore : IAppFlow {

        protected const string DEFAULT_FOLDER = "AppFlowEvents";
        public KeyValueStoreTypeAdapter<AppFlowEvent> store;

        public AppFlowToStore(KeyValueStoreTypeAdapter<AppFlowEvent> store = null) {
            this.store = store == null ? FileBasedKeyValueStore.New(DEFAULT_FOLDER).GetTypeAdapter<AppFlowEvent>() : store;
        }

        public void TrackEvent(string category, string action, params object[] args) {
            var e = new AppFlowEvent() { cat = category, action = action, args = args };
            store.Set(e.time + "__" + category + ":" + action, e).OnError((exeption) => {
                Log.e(exeption);
                return Task.FromException(exeption);
            });
        }

    }

}
