using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace com.csutil {

    public static class LocalizationExtensions {

        public static void textLocalized(this InputField self, string key, params object[] args) {
            I18n i18n = I18n.instance(self);
            if (i18n == null) { i18n = SetupDefaultI18nInstance(self).Result; }
            var localizedText = i18n.Get(key, args);
            if (localizedText != self.text) { self.text = localizedText; }
        }

        public static void textLocalized(this Text self, string key, params object[] args) {
            I18n i18n = I18n.instance(self);
            if (i18n == null) { i18n = SetupDefaultI18nInstance(self).Result; }
            var localizedText = i18n.Get(key, args);
            if (localizedText != self.text) { self.text = localizedText; }
        }

        private static async Task<I18n> SetupDefaultI18nInstance(object caller) {
            var a = await new I18n().SetLocaleLoader(DefaultUnityLocaleLoader);
            return IoC.inject.SetSingleton<I18n>(caller, a);
        }

        private static Task<Dictionary<string, I18n.Translation>> DefaultUnityLocaleLoader(string localeToLoad) {
            try {
                var listJson = ResourcesV2.LoadV2<string>("Locales/" + localeToLoad);
                var list = JsonReader.GetReader().Read<List<I18n.Translation>>(listJson);
                return Task.FromResult(list.ToDictionary(e => e.key, e => e));
            } catch (Exception e) { Log.w("Could not load json for locale=" + localeToLoad, e); }
            return Task.FromResult<Dictionary<string, I18n.Translation>>(null);
        }

    }

}
