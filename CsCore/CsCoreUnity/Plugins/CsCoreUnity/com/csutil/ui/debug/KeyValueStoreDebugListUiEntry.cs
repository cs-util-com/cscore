using ReuseScroller;
using UnityEngine.UI;

namespace com.csutil.ui.debug {
    
    public class KeyValueStoreDebugListUiEntry : BaseCell<KeyValueStoreEntry> {

        public Text keyUi;
        public Text valueUi;

        public override void UpdateContent(KeyValueStoreEntry item) {
            keyUi.text = item.key;
            valueUi.text = "" + item.value;
        }
        
    }
    
}