using com.csutil.encryption;
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.csutil.editor {

    class UnityAccountLoginHelper : AssetPostprocessor {

        // Just to make it harder to understand the json file on disk that contains the user details:
        const string jsonEncryptionKey = "spfvwnsregpwrgp34645345";

        private class LoginDetails { public string email = ""; public string pw = ""; }

        private class UiWindow : EditorWindow { public Action onGUI; void OnGUI() { onGUI(); } }

        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod() {
            UnitySetup.SetupDefaultSingletonsIfNeeded();
            var unityConnect = GetUnityConnect();

            LoginDetails loginDetails = ReadLoginDetails();
            if (loginDetails == null) {
                loginDetails = new LoginDetails() { email = GetUserEmail(unityConnect) };
                SaveLoginDetails(loginDetails);
            }

            if (IsUserLoggedIn(unityConnect)) {
                // If the password is not entered show the input UI:
                if (loginDetails.pw.IsNullOrEmpty()) { ShowLoginDetailsWindow(loginDetails, () => SaveLoginDetails(loginDetails)); }
            } else { ShowLoginDetailsWindow(loginDetails, () => SaveLoginDetails(loginDetails)); }
        }

        private static LoginDetails ReadLoginDetails() {
            try { return JsonReader.GetReader().Read<LoginDetails>(GetFile().LoadAs<string>().Decrypt(jsonEncryptionKey)); }
            catch (Exception e) { Log.w("" + e); return null; }
        }

        private static void ShowLoginDetailsWindow(LoginDetails loginDetails, Action onSave) {
            UiWindow uiWindow = ScriptableObject.CreateInstance<UiWindow>();
            uiWindow.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            uiWindow.onGUI = () => {
                EditorGUILayout.LabelField("Save your Unity Login information for quick access", EditorStyles.wordWrappedLabel);
                loginDetails.email = GUILayout.TextField(loginDetails.email, 25);
                if (GUILayout.Button("Copy to clipboard")) { GUIUtility.systemCopyBuffer = loginDetails.email; }
                loginDetails.pw = GUILayout.PasswordField(loginDetails.pw, "*"[0], 25);
                if (GUILayout.Button("Copy to clipboard")) { GUIUtility.systemCopyBuffer = loginDetails.pw; }
                if (GUILayout.Button("Save")) { onSave(); uiWindow.Close(); }
            };
            uiWindow.Show();
        }

        private static object GetUnityConnect() {
            Assembly assembly = Assembly.GetAssembly(typeof(EditorWindow));
            return assembly.CreateInstance("UnityEditor.Connect.UnityConnect", false, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null, null);
        }

        private static void SaveLoginDetails(LoginDetails l) {
            var j2 = JsonWriter.GetWriter().Write(l);
            GetFile().SaveAsText(j2.Encrypt(jsonEncryptionKey));
        }

        private static FileInfo GetFile() { return EnvironmentV2.instance.GetAppDataFolder().GetChildDir("UnitySettings").GetChild("lhsettings"); }

        private static string GetUserEmail(object uc) { return GetProp<string>(GetProp<object>(uc, "userInfo"), "userName"); }

        private static bool IsUserLoggedIn(object uc) { return GetProp<bool>(uc, "loggedIn"); }

        private static T GetProp<T>(object o, string propName) { return (T)o.GetType().GetProperty(propName).GetValue(o, null); }

    }

}
