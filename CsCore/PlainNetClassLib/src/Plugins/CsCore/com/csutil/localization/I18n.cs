using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace com.csutil {

    public class I18n { // Initial idea from https://github.com/MoonGateLabs/i18n-unity-csharp/

        public static I18n instance(object caller) { return IoC.inject.Get<I18n>(caller); }

        public bool isWarningLogEnabled = true;
        public string currentLocale { get; private set; } = "en-US";
        private CultureInfo cultureInfo;
        private Func<string, string> localeLoader = TryLoadLocaleFromFile;
        private JObject translationData;

        public I18n SetLocale(string newLocale) {
            currentLocale = newLocale;
            cultureInfo = new CultureInfo(currentLocale);
            LoadLocaleFile();
            return this;
        }

        public I18n SetLocaleLoader(Func<string, string> newLocaleLoader) {
            localeLoader = newLocaleLoader;
            LoadLocaleFile();
            return this;
        }

        private void LoadLocaleFile() {
            if (localeLoader != null) { translationData = JObject.Parse(localeLoader(currentLocale)); }
        }

        public string Get(string key, params object[] args) {
            string translation = key;
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
                translation = string.Format(cultureInfo, translation, args);
            }
            return translation;
        }

        string FindSingularOrPlural(string key, object[] args) {
            var translationOptions = translationData[key];
            string translation = key;
            string singPlurKey;
            // find format to try to use
            switch (GetCountAmount(args)) {
                case 0:
                    singPlurKey = "zero";
                    break;
                case 1:
                    singPlurKey = "one";
                    break;
                default:
                    singPlurKey = "other";
                    break;
            }
            // try to use this plural/singular key
            if (translationOptions[singPlurKey] != null) {
                translation = "" + translationOptions[singPlurKey];
            } else if (isWarningLogEnabled) {
                Log.w("Missing singPlurKey:" + singPlurKey + " for:" + key);
            }
            return translation;
        }

        int GetCountAmount(object[] args) {
            int argOne = 0;
            // If arguments passed, try to parse first one to use as count
            if (args.Length > 0 && IsNumeric(args[0])) {
                argOne = Math.Abs(Convert.ToInt32(args[0]));
                if (argOne == 1 && Math.Abs(Convert.ToDouble(args[0])) != 1) { // Check if arg actually equals one
                    argOne = 2;
                } else { // Check if arg actually equals one:
                    if (argOne == 0 && Math.Abs(Convert.ToDouble(args[0])) != 0) { argOne = 2; }
                }
            }
            return argOne;
        }

        bool IsNumeric(object Expression) {
            if (Expression == null || Expression is DateTime) { return false; }
            if (Expression is short || Expression is int || Expression is long || Expression is decimal || Expression is float || Expression is double || Expression is bool) {
                return true;
            }
            return false;
        }

        private static string TryLoadLocaleFromFile(string localeName) {
            var dir = EnvironmentV2.instance.GetCurrentDirectory().GetChildDir("Locales");
            if (!dir.ExistsV2()) { throw Log.e("Could not find locales file in " + dir); }
            var f = dir.GetChild(localeName);
            if (f.ExistsV2()) { return f.LoadAs<string>(); }
            f = dir.GetChild(localeName + ".json");
            if (f.ExistsV2()) { return f.LoadAs<string>(); }
            throw Log.e("Can't load localization file: " + localeName);
        }

    }

}