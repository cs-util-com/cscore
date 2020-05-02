using com.csutil.json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace com.csutil.editor {

    class UnityPackageInstaller {

        public static bool AddToProjectViaPackageManager(string packageName, string packageVersion) {
            var manifestFile = EditorIO.GetProjectFolder().GetChildDir("Packages").GetChild("manifest.json");
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
                        var s = JsonSerializer.Create(JsonNetSettings.defaultSettings);
                        dependencies.Add(packageName, JToken.FromObject(packageVersion, s));
                        manifestFile.SaveAsText(JsonWriter.AsPrettyString(manifest));
                        return true;
                    }
                } else { Log.e("Dependencies list not found in manifest.json"); }
            } else { Log.e("Manifest.json file not found at " + manifestFile); }
            return false;
        }

    }

}