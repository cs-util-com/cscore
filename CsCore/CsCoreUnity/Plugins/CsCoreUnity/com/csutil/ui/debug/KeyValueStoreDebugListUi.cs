using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using ReuseScroller;
using System.Linq;

namespace com.csutil.ui.debug {

    public class KeyValueStoreDebugListUi : BaseController<KeyValueStoreEntry> {

        public async Task SetupKeyValueStoreToShow(ObservableKeyValueStore keyValueStoreToMonitor) {
            var keys = await keyValueStoreToMonitor.GetAllKeys();
            var mapping = await keys.MapAsync(async key => new KeyValueStoreEntry(key, await keyValueStoreToMonitor.Get<object>(key, null)));
            this.CellData = mapping.ToList();
            keyValueStoreToMonitor.CollectionChanged += OnKeyValueStoreChanged;
        }

        private void OnKeyValueStoreChanged(object sender, NotifyCollectionChangedEventArgs e) {
            var store = sender as ObservableKeyValueStore;
            AssertV3.IsNotNull(store, "sender as ObservableKeyValueStore");
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    CellData.Add(ToEntry(e.NewItems));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var toDelete = GetSingleEntry(e.OldItems);
                    CellData.RemoveAt(GetIndexOf(CellData, toDelete.Key));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var updatedEntry = GetSingleEntry(e.NewItems);
                    CellData[GetIndexOf(CellData, updatedEntry.Key)] = ToEntry(updatedEntry);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    CellData.Clear();
                    break;
                default: throw Log.e("Unhandled action " + e.Action);
            }
            this.ReloadData();
        }

        private int GetIndexOf(List<KeyValueStoreEntry> list, string keyToFind) {
            return list.IndexOf(x => x.key == keyToFind);
        }

        private KeyValueStoreEntry ToEntry(IList updatedItems) {
            return ToEntry(GetSingleEntry(updatedItems));
        }

        private static KeyValueStoreEntry ToEntry(KeyValuePair<string, object> e) {
            return new KeyValueStoreEntry(e.Key, e.Value);
        }

        private static KeyValuePair<string, object> GetSingleEntry(IList updatedItems) {
            AssertV3.AreEqual(1, updatedItems.Count);
            var pair = (KeyValuePair<string, object>)updatedItems[0];
            return pair;
        }

    }

    public class KeyValueStoreEntry {
        public string key;
        public object value;
        public KeyValueStoreEntry(string key, object value) {
            this.key = key;
            this.value = value;
        }
    }
    
}