using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis;

namespace com.csutil.keyvaluestore {

    public class GoogleSheetsKeyValueStore : IKeyValueStore {

        public IKeyValueStore fallbackStore { get; set; }
        public long latestFallbackGetTimingInMs { get { return fallbackStore.latestFallbackGetTimingInMs; } set { } }

        private readonly string apiKey;
        private readonly string spreadsheetId;
        private readonly string sheetName;

        public readonly Func<Task<bool>> dowloadOnlineDataDebounced;
        public List<List<string>> latestRawSheetData { get; private set; }

        public GoogleSheetsKeyValueStore(IKeyValueStore store, string apiKey,
                    string spreadsheetId, string sheetName, double delayInMsBetweenCheck = 10000) {
            this.fallbackStore = store;
            this.apiKey = apiKey;
            this.spreadsheetId = spreadsheetId;
            this.sheetName = sheetName;

            // Create a debounced func that only downloads new data max every 10 seconds and
            // trigger this method only if inet available:
            Func<object, Task> debouncedFunc = DowloadOnlineData;
            debouncedFunc = debouncedFunc.AsThrottledDebounce(delayInMsBetweenCheck);
            dowloadOnlineDataDebounced = async () => {
                if (latestRawSheetData.IsNullOrEmpty()) { await InternetStateManager.Instance(this).HasInetAsync; }
                if (InternetStateManager.Instance(this).HasInet) {
                    var downloadTask = debouncedFunc(null);
                    if (downloadTask != null) { await downloadTask; return true; }
                }
                return false;
            };
        }

        public void Dispose() { fallbackStore.Dispose(); }

        private async Task DowloadOnlineData(object _) {
            var newRawSheetData = await GoogleSheetsV4.GetSheet(apiKey, spreadsheetId, sheetName);
            if (!latestRawSheetData.IsNullOrEmpty()) {
                foreach (var entry in ParseRawSheetData(FilterForChanges(latestRawSheetData, newRawSheetData))) {
                    await fallbackStore.Set(entry.Key, entry.Value);
                }
            } else { // Assuming that write is much more expensive then read:
                foreach (var newEntry in ParseRawSheetData(newRawSheetData)) {
                    var oldEntry = await fallbackStore.Get<object>(newEntry.Key, null);
                    if (!JsonWriter.HasEqualJson(oldEntry, newEntry)) {
                        await fallbackStore.Set(newEntry.Key, newEntry.Value);
                    }
                }
            }
            latestRawSheetData = newRawSheetData;
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
            for (int i = 0; i < newLine.Count; i++) { if (oldLine[i] != newLine[i]) { return true; } }
            return false;
        }

        private Dictionary<string, object> ParseRawSheetData(List<List<string>> rawSheetData) {
            var result = new Dictionary<string, object>();
            if (rawSheetData.IsNullOrEmpty()) { return result; }
            var fieldNames = rawSheetData.First().Skip(1).ToList();
            foreach (var column in rawSheetData.Skip(1)) {
                result.Add(column.First(), ToObject(fieldNames, column.Skip(1).ToList()));
            }
            return result;
        }

        private object ToObject(List<string> names, List<string> values) {
            var nc = names.Count();
            var vc = values.Count();
            if (nc < vc) { throw new IndexOutOfRangeException($"Only {nc} names but {vc} values in row"); }
            var result = new Dictionary<string, object>();
            var jsonReader = JsonReader.GetReader();
            for (int i = 0; i < vc; i++) {
                var fieldName = names[i];
                var value = values[i].Trim();
                if (value.IsNullOrEmpty()) { continue; }

                if (value.StartsWith("{")) {
                    result.Add(fieldName, jsonReader.Read<Dictionary<string, object>>(value));
                } else if (value.StartsWith("[")) {
                    result.Add(fieldName, jsonReader.Read<List<object>>(value));
                } else {
                    result.Add(fieldName, ParsePrimitive(value));
                }
            }
            return result;
        }

        private object ParsePrimitive(string value) {
            if (bool.TryParse(value, out bool b)) { return b; }
            if (int.TryParse(value, out int i)) { return i; }
            if (double.TryParse(value, out double d)) { return d; }
            return value;
        }

        public async Task<bool> ContainsKey(string key) {
            await dowloadOnlineDataDebounced();
            return await fallbackStore.ContainsKey(key);
        }

        public async Task<T> Get<T>(string key, T defaultValue) {
            await dowloadOnlineDataDebounced();
            return Mapper.Map<T>(await fallbackStore.Get<object>(key, defaultValue));
        }

        public async Task<IEnumerable<string>> GetAllKeys() {
            await dowloadOnlineDataDebounced();
            return await fallbackStore.GetAllKeys();
        }

        public Task<bool> Remove(string key) { throw new NotSupportedException(); }

        public Task RemoveAll() { throw new NotSupportedException(); }

        public Task<object> Set(string key, object value) { throw new NotSupportedException(); }

    }

}