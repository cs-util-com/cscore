using System.Threading.Tasks;
using com.csutil.keyvaluestore;

namespace com.csutil.logging.analytics {

    public class AppFlowToStore : IAppFlow {

        protected const string DEFAULT_FOLDER = "AppFlowEvents";
        public KeyValueStoreTypeAdapter<AppFlowEvent> store;

        public AppFlowToStore(KeyValueStoreTypeAdapter<AppFlowEvent> store = null) {
            if (store == null) {
                store = FileBasedKeyValueStore.New(DEFAULT_FOLDER).GetTypeAdapter<AppFlowEvent>();
            }
            this.store = store;
        }

        public void TrackEvent(string category, string action, params object[] args) {
            var e = new AppFlowEvent() { cat = category, action = action, args = args };
            store.Set(e.time + "__" + category + "-" + action + ".json", e).LogOnError();
        }

    }

}
