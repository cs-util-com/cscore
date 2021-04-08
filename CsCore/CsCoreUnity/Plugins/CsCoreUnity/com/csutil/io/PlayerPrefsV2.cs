using System;
using UnityEngine;

namespace com.csutil {

    /// <summary> Extends the default PlayerPrefs with some additional functionality and fixes.
    /// 
    /// Consider using <see cref="Preferences.instance"/> over PlayerPrefsV2 to have 
    /// Unity independent key value storage API that can use the PlayerPrefs internally but 
    /// can also switch to other stores easily </summary>
    public class PlayerPrefsV2 : PlayerPrefs {

        public static void SetBool(string key, bool value) {
            SetInt(key, BoolToInt(value));
        }

        public static new string GetString(string key, string defaultValue) {
            var res = PlayerPrefs.GetString(key, defaultValue);
            if (res == "" && !HasKey(key)) { return defaultValue; }
            return res;
        }

        public static bool GetBool(string key, bool defaultValue) {
            return IntToBool(GetInt(key, BoolToInt(defaultValue)));
        }

        private static bool IntToBool(int i) { return i == 0 ? false : true; }
        private static int BoolToInt(bool b) { return b ? 1 : 0; }

        public static void SetStringEncrypted(string key, string value, string password) {
            SetString(key, value.Encrypt(password));
        }

        public static string GetStringDecrypted(string key, string defaultValue, string password) {
            try { if (HasKey(key)) { return GetString(key, defaultValue).Decrypt(password); } }
            catch (Exception e) { Log.w("" + e); }
            return defaultValue;
        }

        public static T GetObject<T>(string key, T defaultValue) {
            if (!HasKey(key)) { return defaultValue; }
            return JsonReader.GetReader().Read<T>(GetString(key));
        }

        public static void SetObject(string key, object obj) {
            SetString(key, JsonWriter.GetWriter().Write(obj));
        }

    }

}
