using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil {

    public interface IPreferences : IKeyValueStore {
        long GetFirstStartDate();
        long GetLastUpdateDate();
    }

    public class Preferences : IPreferences {

        public static IPreferences instance { get { return IoC.inject.Get<IPreferences>(null); } }

        private const string FIRST_START_DATE = "firstStartDate";
        private const string LAST_UPDATE_DATE = "lastUpdateDate";
        private const string LAST_UPDATE_VERSION = "lastUpdateVersion";

        public IKeyValueStore fallbackStore { get; set; }
        public long latestFallbackGetTimingInMs {
            get { return fallbackStore.latestFallbackGetTimingInMs; }
            set { fallbackStore.latestFallbackGetTimingInMs = value; }
        }

        private long? cachedLastUpdateDate;
        private long? cachedFirstAppLaunchDate;

        public Preferences(IKeyValueStore store) {
            this.fallbackStore = store;
            UpdateAppVersionIfNeeded().LogOnError();
        }

        public Task<bool> ContainsKey(string key) { return fallbackStore.ContainsKey(key); }
        public Task<T> Get<T>(string k, T def) { return fallbackStore.Get(k, def); }
        public Task<IEnumerable<string>> GetAllKeys() { return fallbackStore.GetAllKeys(); }
        public Task<bool> Remove(string key) { return fallbackStore.Remove(key); }
        public Task RemoveAll() { return fallbackStore.RemoveAll(); }
        public Task<object> Set(string key, object value) { return fallbackStore.Set(key, value); }

        public void Dispose() {
            fallbackStore.Dispose();
            fallbackStore = null;
            cachedFirstAppLaunchDate = null;
            cachedLastUpdateDate = null;
        }

        public long GetFirstStartDate() {
            if (cachedFirstAppLaunchDate == null) { LoadFirstAppLaunchDate().LogOnError(); }
            return cachedFirstAppLaunchDate.GetValueOrDefault();
        }

        public long GetLastUpdateDate() {
            if (cachedLastUpdateDate == null) { LoadLastUpdateDateFromStore().LogOnError(); }
            return cachedLastUpdateDate.GetValueOrDefault();
        }

        private async Task LoadFirstAppLaunchDate() { cachedFirstAppLaunchDate = await LoadOrInit(FIRST_START_DATE); }

        private async Task LoadLastUpdateDateFromStore() { cachedLastUpdateDate = await LoadOrInit(LAST_UPDATE_DATE); }

        private async Task<long> LoadOrInit(string dateKey) {
            var storedVal = await fallbackStore.Get<long>(dateKey, 0);
            if (storedVal == 0) { // Date was never stored before so save it now
                storedVal = DateTimeV2.UtcNow.ToUnixTimestampUtc();
                await fallbackStore.Set(dateKey, storedVal);
            }
            return storedVal;
        }

        private async Task UpdateAppVersionIfNeeded() {
            var storedVersion = await fallbackStore.Get<string>(LAST_UPDATE_VERSION, null);
            var currentVersion = EnvironmentV2.instance.systemInfo.appVersion;
            if (currentVersion != storedVersion) {
                await fallbackStore.Set(LAST_UPDATE_VERSION, currentVersion);
                // Every time the app version must be updated also set the new date:
                cachedLastUpdateDate = DateTimeV2.UtcNow.ToUnixTimestampUtc();
                await fallbackStore.Set(LAST_UPDATE_DATE, cachedLastUpdateDate);
                EventBus.instance.Publish(EventConsts.catSystem + EventConsts.APP_VERSION_CHANGED, storedVersion, currentVersion);
            }
        }

    }

}