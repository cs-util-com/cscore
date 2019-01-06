using com.csutil.encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public class PlayerPrefsV2 : PlayerPrefs {

        public static void SetBool(string key, bool value) {
            SetInt(key, BoolToInt(value));
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
            try { return GetString(key, defaultValue).Decrypt(password); }
            catch (Exception e) { Log.w("" + e); }
            return defaultValue;
        }

    }

}
