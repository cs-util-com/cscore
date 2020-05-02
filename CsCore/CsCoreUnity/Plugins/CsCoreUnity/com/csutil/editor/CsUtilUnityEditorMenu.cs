using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Zio;

namespace com.csutil.editor {

    public class CsUtilUnityEditorMenu {

        private const string DIR = "Window/CsUtil/";

        [MenuItem(DIR + "Fonts/Open MaterialUi Icon Overview")]
        static void OpenMaterialIcons() { Application.OpenURL("https://shanfan.github.io/material-icons-cheatsheet/"); }

        [MenuItem(DIR + "Fonts/Open FontAwesome Icon Overview")]
        static void OpenFontAwesomeIcons() { Application.OpenURL("https://fontawesome.com/cheatsheet"); }

        [MenuItem(DIR + "CsCore/Open GitHub page (Documentation)")]
        static void CsCoreGithubPage() { Application.OpenURL("https://github.com/cs-util-com/cscore"); }

        [MenuItem(DIR + "CsCore/Report a problem")]
        static void ReportCsCoreProblem() { Application.OpenURL("https://github.com/cs-util-com/cscore/issues"); }

        [MenuItem(DIR + "CsCore/Install default packages")]
        static void InstallDefaultPackages() {
            UnityPackageInstaller.AddToProjectViaPackageManager("com.unity.build-report-inspector", "0.1.2-preview");
            UnityPackageInstaller.AddToProjectViaPackageManager("com.unity.device-simulator", "2.0.0-preview");
            UnityPackageInstaller.AddToProjectViaPackageManager("com.unity.mobile.android-logcat", "1.0.0");
            // UnityPackageInstaller.AddToProjectViaPackageManager("com.unity.nuget.newtonsoft-json", "2.0.0-preview");
            // UnityPackageInstaller.AddToProjectViaPackageManager("com.unity.vectorgraphics", "2.0.0-preview.11");
        }

        [MenuItem(DIR + "Show Asset Store packages")]
        static void ShowAssetStore() { Application.OpenURL("https://assetstore.unity.com/publishers/40989"); }

        [MenuItem("CONTEXT/RectTransform/Set Anchors Around Object")]
        static void SetAnchorsAroundObject(MenuCommand command) {
            SetAnchorsAroundObject(command.context as RectTransform);
        }

        static void SetAnchorsAroundObject(RectTransform t) {
            if (!t.transform.parent) { return; }
            Rect pT = t.transform.parent.GetComponentV2<RectTransform>().rect;
            t.anchorMin = new Vector2(t.anchorMin.x + (t.offsetMin.x / pT.width), t.anchorMin.y + (t.offsetMin.y / pT.height));
            t.anchorMax = new Vector2(t.anchorMax.x + (t.offsetMax.x / pT.width), t.anchorMax.y + (t.offsetMax.y / pT.height));
            t.offsetMin = Vector2.zero;
            t.offsetMax = Vector2.zero;
            t.pivot = new Vector2(0.5f, 0.5f);
            t.pivot = new Vector2(0.5f, 0.5f);
        }

        [MenuItem(DIR + "Editor Colors/Set Playmode-Tint to red")]
        static void SetPlayModeTintToLightRed() {
            Color lightRed = ColorUtil.HexStringToColor("#FFCDC5");
            EditorPrefsColors.SetPlaymodeTintColor(lightRed);
        }

        [MenuItem(DIR + "Editor Colors/Set UI-Canvas-Background to blue")]
        static void SetSceneBackgroundColor() {
            Color lightBlue = ColorUtil.HexStringToColor("#93BAF4");
            EditorPrefsColors.SetSceneBackgroundColor(lightBlue);
        }

        [MenuItem(DIR + "Editor Colors/Set Main-Camera-background to yellow")]
        static void SetMainCamBackgroundColor() {
            var lightYellow = ColorUtil.HexStringToColor("#FEFFE4");
            Camera.main.backgroundColor = lightYellow;
        }

        [MenuItem(DIR + "Create default Unity .gitignore files")]
        static void CreateDefaultGitIgnoreFiles() {
            var projectFolder = EditorIO.GetProjectFolder();
            var assetsFolder = EditorIO.GetAssetsFolder();
            if (!assetsFolder.Exists) { throw Log.e("Not the project folder: " + projectFolder); }
            var file = projectFolder.GetChild(".gitignore");
            if (!file.Exists) {
                EditorCoroutineRunner.StartCoroutine(DownloadDefaultUnityGitIgnore(file));
            } else {
                Log.d("No need to download .gitignore, was already found: " + file);
            }
            GitIgnoreUdater.AddAllSymlinksToGitIgnores(assetsFolder);
            AssetFolderAnalysis.FindFolderAnomalies(assetsFolder);
        }

        private static IEnumerator DownloadDefaultUnityGitIgnore(FileEntry file) {
            var request = UnityWebRequest.Get("https://raw.githubusercontent.com/github/gitignore/master/Unity.gitignore");
            yield return request.SendWebRequest();
            while (!request.isDone) { yield return new WaitForSeconds(0.1f); }
            var gitignoreContent = request.GetResult<string>();
            if (!gitignoreContent.IsNullOrEmpty()) { file.SaveAsText(gitignoreContent); }
            if (file.Exists) {
                Log.d("Successfull downloaded gitignore into file=" + file);
            } else {
                Log.e("Could not donwload  gitignore into file=" + file);
            }
        }

        private class GitIgnoreUdater {

            public static void AddAllSymlinksToGitIgnores(DirectoryEntry assetsFolder) {
                var allSymbolicLinks = CollectSymbolicLinkedFolders(assetsFolder);
                foreach (var symLink in allSymbolicLinks) {
                    var gitignore = symLink.Parent.GetChild(".gitignore");
                    var symLinkFolderName = "/" + symLink.Name;
                    if (AddLineToGitIngore(gitignore, symLinkFolderName)) {
                        Log.d("Added entry to gitignore " + gitignore + " :\n" + symLinkFolderName + "\n");
                    }
                }
            }

            private static List<DirectoryEntry> CollectSymbolicLinkedFolders(DirectoryEntry assetsFolder) {
                var links = new List<DirectoryEntry>();
                var normalFolders = assetsFolder.GetDirectories().Filter(dir => {
                    if (!dir.Name.IsNullOrEmpty() && IsSymbolicLink(dir)) { links.Add(dir); return false; }
                    return true;
                }); // Then visit all normal children folders to search there too:
                foreach (var dir in normalFolders) { links.AddRange(CollectSymbolicLinkedFolders(dir)); }
                return links;
            }

            private static bool IsSymbolicLink(DirectoryEntry dir) {
                return dir.Attributes.HasFlag(FileAttributes.ReparsePoint);
            }

            private static bool AddLineToGitIngore(FileEntry gitignore, string symLinkFolderNameToAdd) {
                var lines = gitignore.Exists ? File.ReadLines(gitignore.FullName) : new string[0];
                var found = lines.Any(line => symLinkFolderNameToAdd.Equals(line));
                if (!found) { File.AppendAllLines(gitignore.FullName, new string[2] { "", symLinkFolderNameToAdd }); }
                return !found;
            }

        }

        private class AssetFolderAnalysis {

            public static void FindFolderAnomalies(DirectoryEntry folder) {
                if (Equals(0, folder.GetFiles().Count())) { Log.e("Emtpy folder found: " + folder); }
                foreach (var childDir in folder.GetDirectories()) { FindFolderAnomalies(childDir); }
            }

        }

    }

}