using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CsUtilUnityEditorMenu {

    [UnityEditor.MenuItem("CsUtil/Open MaterialUi Icon Overview")]
    static void OpenMaterialUiIconOverview() { Application.OpenURL("https://shanfan.github.io/material-icons-cheatsheet/"); }

    [UnityEditor.MenuItem("CsUtil/CsCore/Open GitHub page")]
    static void CsCoreGithubPage() { Application.OpenURL("https://github.com/cs-util-com/cscore"); }

    [UnityEditor.MenuItem("CsUtil/CsCore/Report a problem")]
    static void ReportCsCoreProblem() { Application.OpenURL("https://github.com/cs-util-com/cscore/issues"); }

    [UnityEditor.MenuItem("CsUtil/Show all Asset Store packages")]
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

}
