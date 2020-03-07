using System;
using UnityEngine.UI;

namespace com.csutil {

    public static class LocalizationExtensions {

        public static void textLocalized(this Text self, string key, params object[] args) {
            I18n i18n = I18n.instance(self);
            if (i18n == null) { i18n = SetupDefaultI18nInstance(self); }
            var localizedText = i18n.Get(key, args);
            if (localizedText != self.text) { self.text = localizedText; }
        }

        private static I18n SetupDefaultI18nInstance(object caller) {
            return IoC.inject.SetSingleton(caller, new I18n().SetLocaleLoader(DefaultUnityLocaleLoader));
        }

        private static string DefaultUnityLocaleLoader(string localeToLoad) {
            try { return ResourcesV2.LoadV2<string>("Locales/" + localeToLoad); } catch (Exception e) { Log.w("Could not load json for locale=" + localeToLoad, e); }
            return null;
        }

    }

}
