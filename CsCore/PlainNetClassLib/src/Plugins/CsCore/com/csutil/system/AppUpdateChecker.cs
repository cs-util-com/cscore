using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.csutil.system {

    public class DefaultAppUpdateChecker : AppUpdateChecker {

        private IKeyValueStore store;

        public DefaultAppUpdateChecker(IKeyValueStore store, Action<List<UpdateEntry>> onResult) : base(onResult) {
            this.store = store;
        }

        public override async Task<IEnumerable<UpdateEntry>> DownloadAllUpdateEntries() {
            return await (await store.GetAllKeys()).MapAsync(key => store.Get<UpdateEntry>(key, null));
        }

    }

    public abstract class AppUpdateChecker : IHasInternetListener, IDisposable {

        public Action<List<UpdateEntry>> showUserUpdateInstructions;
        private bool hasTriedToCheckForUpdate;

        public AppUpdateChecker(Action<List<UpdateEntry>> showUserUpdateInstructions) {
            this.showUserUpdateInstructions = showUserUpdateInstructions;
            InternetStateManager.AddListener(this);
        }

        public void Dispose() { InternetStateManager.RemoveListener(this); }

        public async Task OnHasInternet(bool hasInet) {
            if (hasInet && !hasTriedToCheckForUpdate && showUserUpdateInstructions != null) {
                hasTriedToCheckForUpdate = true; // set it first to only try once per app lifecycle
                var matchingUpdateEntries = await DownloadMatchingUpdateEntries();
                AssertV2.IsTrue(matchingUpdateEntries.Count() < 2, "More then one matching update entry!");
                if (!matchingUpdateEntries.IsNullOrEmpty()) {
                    showUserUpdateInstructions(matchingUpdateEntries);
                }
            }
        }

        public async Task<List<UpdateEntry>> DownloadMatchingUpdateEntries() {
            IEnumerable<UpdateEntry> entries = await DownloadAllUpdateEntries();
            var sysInfos = Mapper.Map<Dictionary<string, string>>(EnvironmentV2.instance.systemInfo);
            return entries.Filter(e => MatchesConditions(e, sysInfos)).ToList();
        }

        private bool MatchesConditions(UpdateEntry entry, Dictionary<string, string> systemInfos) {
            foreach (var attribute in entry) {
                if (systemInfos.ContainsKey(attribute.Key)) {
                    var valueMatches = attribute.Value.ToString() != systemInfos[attribute.Key];
                    if (valueMatches) { return false; }
                }
            }
            return true; // None of the conditions did not match the entry
        }

        public abstract Task<IEnumerable<UpdateEntry>> DownloadAllUpdateEntries();

        /// <summary> 
        /// An update entry is an arbitrary list of conditions that have to be met by the client plus a mandatory
        /// string field "updateInstructions" that contains a JSON string with all the update instructions,
        /// and this JSON can be parsed into the UpdateInstructions type to get access to all its fields.
        /// </summary>
        public class UpdateEntry : Dictionary<string, object> {
            public UpdateInstructions GetUpdateInstructions() {
                return Mapper.Map<UpdateInstructions>(this["updateInstructions"]);
            }
        }

        public class UpdateInstructions {
            public string appVersion { get; set; }
            public string url { get; set; }
            public string downloadUrl { get; set; }
            public string title { get; set; }
            public string notes { get; set; }
            public bool mandatory { get; set; }
            public string releaseDate { get; set; }

            public DateTime GetReleaseDate() { return DateTimeV2.ParseUtc(releaseDate); }

        }

    }

}