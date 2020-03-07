using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace com.csutil {

    public class I18n { // Initial idea from https://github.com/MoonGateLabs/i18n-unity-csharp/

        public static I18n instance(object caller) { return IoC.inject.Get<I18n>(caller); }

        public bool isWarningLogEnabled = true;
        public string currentLocale { get; private set; } = "" + CultureInfo.CurrentUICulture;
        public CultureInfo currentCulture = CultureInfo.CurrentCulture;
        private Func<string, string> localeLoader = TryLoadLocaleFromFile;
        private JObject translationData;

        public I18n SetLocale(string newLocale) {
            currentLocale = newLocale;
            currentCulture = new CultureInfo(currentLocale);
            LoadTranslationDataForLocale();
            return this;
        }

        public I18n SetLocaleLoader(Func<string, string> newLocaleLoader) {
            localeLoader = newLocaleLoader;
            LoadTranslationDataForLocale();
            return this;
        }

        private void LoadTranslationDataForLocale() {
            if (localeLoader != null) { translationData = JObject.Parse(localeLoader(currentLocale)); }
        }

        public string Get(string key, params object[] args) {
            string translation = key;
            if (translationData == null) { LoadTranslationDataForLocale(); }
            if (translationData?[key] != null) {
                // if this key is a direct string
                if (translationData[key].Count() == 0) {
                    translation = "" + translationData[key];
                } else {
                    translation = FindSingularOrPlural(key, args);
                }
            } else if (isWarningLogEnabled) { Log.w("Could not translate: " + key); }
            // check if we have embeddable data
            if (args.Length > 0) {
                translation = string.Format(currentCulture, translation, args);
            }
            return translation;
        }

        string FindSingularOrPlural(string key, object[] args) {
            var translationOptions = translationData[key];
            string translation = key;
            string singPlurKey = GetKey(args);
            // try to use this plural/singular key
            if (translationOptions[singPlurKey] != null) {
                translation = "" + translationOptions[singPlurKey];
            } else if (isWarningLogEnabled) {
                Log.w("Missing singPlurKey:" + singPlurKey + " for:" + key);
            }
            return translation;
        }

        private string GetKey(object[] args) {
            if (GetNumberInArgs(args) == 0) { return "zero"; }
            if (GetNumberInArgs(args) == 1) { return "one"; }
            return "other";
        }

        int GetNumberInArgs(object[] args) {
            var firstNumberFound = args.First(a => IsNumeric(a));
            var argOne = Math.Abs(Convert.ToInt32(firstNumberFound));
            if (argOne == 0 && Math.Abs(Convert.ToDouble(firstNumberFound)) != 0) { return 2; }
            if (argOne == 1 && Math.Abs(Convert.ToDouble(firstNumberFound)) != 1) { return 2; }
            return argOne;
        }

        bool IsNumeric(object Expression) {
            if (Expression == null || Expression is DateTime) { return false; }
            return Expression is short || Expression is int || Expression is long
                    || Expression is decimal || Expression is float
                    || Expression is double || Expression is bool;
        }

        private static string TryLoadLocaleFromFile(string localeName) {
            var dir = EnvironmentV2.instance.GetCurrentDirectory().GetChildDir("Locales");
            var f = dir.GetChild(localeName);
            if (f.ExistsV2()) { return f.LoadAs<string>(); }
            f = dir.GetChild(localeName + ".json");
            if (f.ExistsV2()) { return f.LoadAs<string>(); }
            Log.e("Can't load localization file: " + localeName);
            return null;
        }

    }

}