using UnityEngine;

namespace com.csutil.editor {

    public class CsUtilUnityEditorMenu {

        private const string DIR = "Window/CsUtil/";

        [UnityEditor.MenuItem(DIR + "Open MaterialUi Icon Overview")]
        static void OpenMaterialUiIconOverview() { Application.OpenURL("https://shanfan.github.io/material-icons-cheatsheet/"); }

        [UnityEditor.MenuItem(DIR + "CsCore/Open GitHub page (Documentation)")]
        static void CsCoreGithubPage() { Application.OpenURL("https://github.com/cs-util-com/cscore"); }

        [UnityEditor.MenuItem(DIR + "CsCore/Report a problem")]
        static void ReportCsCoreProblem() { Application.OpenURL("https://github.com/cs-util-com/cscore/issues"); }

        [UnityEditor.MenuItem(DIR + "Show Asset Store packages")]
        static void ShowAssetStore() { Application.OpenURL("https://assetstore.unity.com/publishers/40989"); }

        [UnityEditor.MenuItem("CONTEXT/RectTransform/Set Anchors Around Object")]
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

        [UnityEditor.MenuItem(DIR + "Editor Colors/Set PlaymodeTint to red")]
        static void SetPlayModeTintToLightRed() {
            Color lightRed = ColorUtil.HexStringToColor("#FFCDC5");
            EditorPrefsColors.SetPlaymodeTintColor(lightRed);
        }

        [UnityEditor.MenuItem(DIR + "Editor Colors/Set UI canvas background to blue")]
        static void SetSceneBackgroundColor() {
            Color lightBlue = ColorUtil.HexStringToColor("#93BAF4");
            EditorPrefsColors.SetSceneBackgroundColor(lightBlue);
        }

    }

}