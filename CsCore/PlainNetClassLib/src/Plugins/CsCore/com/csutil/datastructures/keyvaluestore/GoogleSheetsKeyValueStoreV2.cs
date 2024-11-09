using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.http.apis;

namespace com.csutil.keyvaluestore {

    public class GoogleSheetsKeyValueStoreV2 : IKeyValueStore {

        public IKeyValueStore fallbackStore { get; set; }
        public long latestFallbackGetTimingInMs { get { return fallbackStore.latestFallbackGetTimingInMs; } set { } }

        private readonly Uri csvUrl;

        public Func<Task<bool>> dowloadOnlineDataDebounced { get; private set; }

        private readonly double delayInMsBetweenCheck;
        private Dictionary<string, object> sheetData { get; set; }

        public GoogleSheetsKeyValueStoreV2(IKeyValueStore localCache, Uri csvUrl, double delayInMsBetweenCheck = 10000) {
            this.fallbackStore = localCache;
            this.delayInMsBetweenCheck = delayInMsBetweenCheck;
            InitDebouncedDownloadLogic();
            this.csvUrl = csvUrl;
        }

        private void InitDebouncedDownloadLogic() {
            sheetData = null;
            // Create a debounced func that only downloads new data max every 10 seconds and
            // trigger this method only if inet available:
            Func<Task> t = async () => {
                if (sheetData.IsNullOrEmpty()) {
                    await InternetStateManager.Instance(this).HasInetAsync;
                }
                if (InternetStateManager.Instance(this).HasInet) {
                    await DowloadOnlineData();
                }
                ThrowIfSheetDataMissing();
            };
            dowloadOnlineDataDebounced = t.AsThrottledDebounceV2(delayInMsBetweenCheck);
        }

        private void ThrowIfSheetDataMissing() {
            if (sheetData == null) { throw new Exception("Could not download Google Sheet data"); }
        }

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;

        public void Dispose() {
            IsDisposed = DisposeState.DisposingStarted;
            fallbackStore?.Dispose();
            IsDisposed = DisposeState.Disposed;
        }

        private async Task DowloadOnlineData() {
            Dictionary<string, object> newSheetData = await GoogleSheetsV5.GetSheetObjects(csvUrl);
            if (!sheetData.IsNullOrEmpty()) {
                foreach (var entry in newSheetData) {
                    await fallbackStore.Set(entry.Key, entry.Value);
                }
            } else { // Assuming that write is much more expensive then read:
                foreach (var newEntry in newSheetData) {
                    var oldEntry = await fallbackStore.Get<object>(newEntry.Key, null);
                    if (!JsonWriter.GetWriter(newEntry).HasEqualJson(oldEntry, newEntry)) {
                        await fallbackStore.Set(newEntry.Key, newEntry.Value);
                    }
                }
            }
            sheetData = newSheetData;
        }

        private async Task DownloadOnlineDataIfNeeded() {
            var t = dowloadOnlineDataDebounced();
            if (sheetData == null) { await t; }
        }

        private static List<List<string>> FilterForChanges(List<List<string>> oldData, List<List<string>> newData) {
            var filtered = new List<List<string>>();
            for (int i = 0; i < newData.Count; i++) {
                var newLine = newData[i];
                if (oldData.Count <= i || ChangeFound(oldData[i], newLine)) { filtered.Add(newLine); }
            }
            return filtered;
        }

        private static bool ChangeFound(List<string> oldLine, List<string> newLine) {
            if (oldLine.Count != newLine.Count) { return true; }
            for (int i = 0; i < newLine.Count; i++) {
                if (oldLine[i] != newLine[i]) { return true; }
            }
            return false;
        }

        public async Task<bool> ContainsKey(string key) {
            await DownloadOnlineDataIfNeeded();
            return await fallbackStore.ContainsKey(key);
        }

        public async Task<T> Get<T>(string key, T defaultValue) {
            await DownloadOnlineDataIfNeeded();
            return Mapper.Map<T>(await fallbackStore.Get<object>(key, defaultValue));
        }

        public async Task<IEnumerable<string>> GetAllKeys() {
            await DownloadOnlineDataIfNeeded();
            return (await fallbackStore.GetAllKeys()).Cached();
        }

        public Task<bool> Remove(string key) { throw new NotSupportedException(this + " is a readonly store"); }

        public Task RemoveAll() { throw new NotSupportedException(this + " is a readonly store"); }

        public Task<object> Set(string key, object value) { throw new NotSupportedException(this + " is a readonly store"); }

    }

}