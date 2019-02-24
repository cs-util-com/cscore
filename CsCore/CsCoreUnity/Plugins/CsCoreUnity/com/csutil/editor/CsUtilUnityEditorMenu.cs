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

}
