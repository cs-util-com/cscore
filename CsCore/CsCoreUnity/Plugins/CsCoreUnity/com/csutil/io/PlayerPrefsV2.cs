using com.csutil.encryption;
using com.csutil.io;
using com.csutil.keyvaluestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public class PlayerPrefsV2 : PlayerPrefs {

        public static IKeyValueStore instance {
            get { return IoC.inject.GetOrAddSingleton(new object(), () => newDefaultSettingsPipeline()); }
        }

        /// <summary> 
        /// THe default store uses a Dictionary for memory caching and an exception layer to catch
        /// and errors that might happen e.g. when the PlayerPrefs are accessed from a background task
        /// </summary>
        private static IKeyValueStore newDefaultSettingsPipeline() {
            var wrappedPlayerPrefs = new ExceptionWrapperKeyValueStore(new PlayerPrefsStore());
            return new InMemoryKeyValueStore().WithFallbackStore(wrappedPlayerPrefs);
        }

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
