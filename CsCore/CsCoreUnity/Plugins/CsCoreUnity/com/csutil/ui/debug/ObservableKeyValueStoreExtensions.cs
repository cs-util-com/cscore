using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using UnityEngine;

namespace com.csutil.ui.debug {

    public static class ObservableKeyValueStoreExtensions {

        public static async Task<GameObject> ShowDebugUiScreen(this ObservableKeyValueStore keyValueStoreToMonitor) {
            var viewStackForKeyValueStoreUi = RootCanvas.GetOrAddRootCanvasV2().GetOrAddViewStack("KeyValueStoreDebugListUiViewStack");
            var debugUiViewStackGo = viewStackForKeyValueStoreUi.gameObject;
            await debugUiViewStackGo.GetLinkMap().Get<KeyValueStoreDebugListUi>("KeyValueStoreDebugListUi").SetupKeyValueStoreToShow(keyValueStoreToMonitor);
            return debugUiViewStackGo;
        }

    }

}