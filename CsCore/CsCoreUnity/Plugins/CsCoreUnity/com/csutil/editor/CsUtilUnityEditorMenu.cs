using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

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

        [MenuItem(DIR + "Show Asset Store packages")]
        static void ShowAssetStore() { Application.OpenURL("https://assetstore.unity.com/publishers/40989"); }

        [MenuItem("CONTEXT/RectTransform/Set Anchors Around Object")]
        static void SetAnchorsAroundObject(UnityEditor.MenuCommand command) {
            SetAnchorsAroundObject(command.context as RectTransform);
        }

        static void SetAnchorsAroundObject(RectTransform t) {
            if (!t.transform.parent) { return; }
            Rect pT = t.transform.parent.GetComponent<RectTransform>().rect;
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

        [MenuItem(DIR + "Download default Unity .gitignore")]
        static void DownloadDefaultUnityGitIgnore() {
            var projectFolder = EnvironmentV2.instance.GetCurrentDirectory();
            if (!projectFolder.GetChildDir("Assets").Exists) { throw Log.e("Not the project folder: " + projectFolder); }
            var file = projectFolder.GetChild(".gitignore");
            if (!file.ExistsV2()) {
                EditorCoroutineRunner.StartCoroutine(DownloadDefaultUnityGitIgnore(file));
            } else {
                Log.d("No need to download .gitignore, was already found: " + file);
            }
        }

        private static IEnumerator DownloadDefaultUnityGitIgnore(FileInfo file) {
            var request = UnityWebRequest.Get("https://raw.githubusercontent.com/github/gitignore/master/Unity.gitignore");
            yield return request.SendWebRequest();
            while (!request.isDone) { yield return new WaitForSeconds(0.1f); }
            var gitignoreContent = request.GetResult<string>();
            if (gitignoreContent.IsNullOrEmpty()) { file.SaveAsText(gitignoreContent); }
            if (file.ExistsV2()) {
                Log.d("Successfull downloaded gitignore into file=" + file);
            } else {
                Log.e("Could not donwload  gitignore into file=" + file);
            }
        }

    }

}