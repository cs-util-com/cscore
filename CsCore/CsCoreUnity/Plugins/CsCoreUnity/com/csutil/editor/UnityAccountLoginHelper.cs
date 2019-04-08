using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.csutil.editor {

    class UnityAccountLoginHelper : AssetPostprocessor {

        private class LoginDetails { public string email = ""; public string pw = ""; }

        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod() {

            UnitySetup.SetupDefaultSingletonsIfNeeded();
            var unityConnect = NewUnityConnect();

            // Just to make it harder to understand the json file on disk that contains the user details:
            const string jsonEncrKey = "eo34546o34o35t438t36z84gto34wto3t4g34t3w24ef";
            LoginDetails loginDetails = GetFile().LoadAsEncyptedJson(jsonEncrKey, () => {
                return new LoginDetails() { email = GetUserEmail(unityConnect) };
            });
            // If the user is not logged in or the password is not entered show the input UI:
            if (!IsUserLoggedIn(unityConnect) || loginDetails.pw.IsNullOrEmpty()) {
                ShowLoginDetailsWindow(loginDetails, () => GetFile().SaveAsEncryptedJson(loginDetails, jsonEncrKey));
            }
        }

        private static FileInfo GetFile() { return EnvironmentV2.instance.GetAppDataFolder().GetChildDir("UnitySettings").GetChild("ualhs"); }

        private static void ShowLoginDetailsWindow(LoginDetails loginDetails, Action onSave) {
            EditorWindowV2.ShowUtilityWindow((EditorWindowV2 ui) => {
                EditorGUILayout.LabelField("Save your Unity Login information for quick access", EditorStyles.wordWrappedLabel);
                loginDetails.email = GUILayout.TextField(loginDetails.email, 25);
                if (GUILayout.Button("Copy to clipboard")) { GUIUtility.systemCopyBuffer = loginDetails.email; }
                loginDetails.pw = GUILayout.PasswordField(loginDetails.pw, "*"[0], 25);
                if (GUILayout.Button("Copy to clipboard")) { GUIUtility.systemCopyBuffer = loginDetails.pw; }
                if (GUILayout.Button("Save")) { onSave(); ui.Close(); }
            });
        }

        private static object NewUnityConnect() {
            Assembly a = Assembly.GetAssembly(typeof(EditorWindow));
            return a.CreateInstance("UnityEditor.Connect.UnityConnect", false, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null, null);
        }

        private static string GetUserEmail(object uc) {
            try { return GetProp<string>(GetProp<object>(uc, "userInfo"), "userName"); }
            catch (Exception e) { Log.w("" + e); return ""; }
        }

        private static bool IsUserLoggedIn(object uc) { return GetProp<bool>(uc, "loggedIn"); }

        private static T GetProp<T>(object o, string propName) { return (T)o.GetType().GetProperty(propName).GetValue(o, null); }

    }

}
