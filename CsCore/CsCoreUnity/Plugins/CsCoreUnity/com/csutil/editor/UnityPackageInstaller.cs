using com.csutil.json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace com.csutil.editor {

    public class UnityPackageInstaller {

        public static bool AddToProjectViaPackageManager(string packageName, string packageVersion) {
            var manifestFile = EditorIO.GetProjectFolder().GetChildDir("Packages").GetChild("manifest.json");
            if (!manifestFile.IsNotNullAndExists()) {
                Log.e("Manifest.json file not found at " + manifestFile);
                return false;
            }
            var manifest = manifestFile.LoadAs<Dictionary<string, object>>();
            JObject dependencies = (manifest["dependencies"] as JObject);
            if (dependencies == null) {
                Log.e("Dependencies list not found in manifest.json");
                return false;
            }
            if (dependencies.TryGetValue(packageName, out JToken foundVersion)) {
                if (packageVersion.Equals(foundVersion.ToObject<string>())) {
                    return true; // Package already contained in Packages/manifest.json
                }
                Log.w($"Package {packageName}:{packageVersion} not added, "
                    + $"found existing version in manifest.json: {foundVersion}");
                return false;
            }
            var s = JsonSerializer.Create(JsonNetSettings.defaultSettings);
            dependencies.Add(packageName, JToken.FromObject(packageVersion, s));
            manifestFile.SaveAsText(JsonWriter.AsPrettyString(manifest));
            return true;
        }

    }

}