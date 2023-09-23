using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using com.csutil.http;
using System.Net.Http;
using System.Collections.Generic;
using UnityEditor;

namespace com.csutil.editor {

    [InitializeOnLoad]
    public class LatestCsCoreVersionChecker {

        static LatestCsCoreVersionChecker() {
            CheckForLatestVersion().LogOnError();
        }

        public static async Task CheckForLatestVersion() {
            do {
                await Task.Delay(3000); // Wait at least 3sec before starting
            } while (!(await InternetStateManager.Instance(null).HasInetAsync)); // Wait until internet awailable

            try {
                var installedPackages = await GetInstalledPackagesFromPackageManager();
                await CheckForUpdates(installedPackages, "https://raw.githubusercontent.com/cs-util-com/cscore/master/CsCore/PlainNetClassLib/src/Plugins/package.json");
                await CheckForUpdates(installedPackages, "https://raw.githubusercontent.com/cs-util-com/cscore/master/CsCore/CsCoreUnity/Plugins/package.json");
            } catch (Exception e) {
                Log.w("Could not check for latest version of CsCore: " + e);
            }
        }

        private static async Task CheckForUpdates(List<UnityEditor.PackageManager.PackageInfo> installedPackages, string urlToLoadPackageJsonFrom) {
            var request = new UriRestRequest(new Uri(urlToLoadPackageJsonFrom)).Send(HttpMethod.Get);
            var latestPackageJson = await request.GetResult<PackageJson>();

            var localPackageJson = installedPackages.SingleOrDefault(localPackage => localPackage.name == latestPackageJson.name);
            if (localPackageJson != null) {
                var localVersion = Version.Parse(localPackageJson.version);
                var remoteVersion = Version.Parse(latestPackageJson.version);
                bool isLocalLibUpToDate = localVersion >= remoteVersion; 
                if (!isLocalLibUpToDate) {
                    Log.w($"UPDATE AVAILABLE: Use the Unity package manager to update {localPackageJson.name} from your v{localVersion} to latest v{remoteVersion}");
                }
            } else {
                Log.i($"{latestPackageJson.name.ToUpperInvariant()} was not included via package manager, this is NOT recommended! See https://github.com/cs-util-com/cscore#install-cscore-into-your-unity-project");
            }
        }

        private static async Task<List<UnityEditor.PackageManager.PackageInfo>> GetInstalledPackagesFromPackageManager() {
            var installedPackagesRequest = UnityEditor.PackageManager.Client.List();
            while (!installedPackagesRequest.IsCompleted) { await TaskV2.Delay(50); }
            if (installedPackagesRequest.Status != UnityEditor.PackageManager.StatusCode.Success) {
                throw new InvalidOperationException("Could not receive installed packages successfully from Unity package manager");
            }
            return installedPackagesRequest.Result.ToList();
        }

        private class PackageJson {

            public string name { get; set; }
            public string version { get; set; }
            public string displayName { get; set; }
            public string description { get; set; }
            public string unity { get; set; }
            public string license { get; set; }
            public Author author { get; set; }
            public Dependencies dependencies { get; set; }

            public class Author {
                public string name { get; set; }
                public string url { get; set; }
            }

            public class Dependencies {
                [JsonProperty("com.unity.nuget.newtonsoft-json")]
                public string ComUnityNugetNewtonsoftJson { get; set; }
            }

        }

    }

}
