using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace com.csutil.editor {

    class UnityPackageInstaller {

        public static bool AddToProjectViaPackageManager(string packageName, string packageVersion) {
            var manifestFile = EnvironmentV2.instance.GetCurrentDirectory().Parent.GetChildDir("Packages").GetChild("manifest.json");
            if (manifestFile.IsNotNullAndExists()) {
                var manifest = manifestFile.LoadAs<Dictionary<string, object>>();
                JObject dependencies = (manifest["dependencies"] as JObject);
                if (dependencies != null) {
                    if (dependencies.TryGetValue(packageName, out JToken foundVersion)) {
                        if (packageVersion.Equals(foundVersion.ToObject<string>())) {
                            // Log.d("Package " + packageName + " already contained in Packages/manifest.json");
                            return true;
                        } else { Log.w("Package " + packageName + ":" + packageVersion + " not added, found exist. version in manifest.json: " + foundVersion); }
                    } else {
                        dependencies.Add(packageName, JToken.FromObject(packageVersion));
                        manifestFile.SaveAsText(JsonWriter.AsPrettyString(manifest));
                        return true;
                    }
                } else { Log.e("Dependencies list not found in manifest.json"); }
            } else { Log.e("Manifest.json file not found at " + manifestFile.FullPath()); }
            return false;
        }

    }

}