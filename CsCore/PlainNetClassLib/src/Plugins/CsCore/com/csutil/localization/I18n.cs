using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using Newtonsoft.Json.Linq;

namespace com.csutil {

    public class I18n { // Initial idea from https://github.com/MoonGateLabs/i18n-unity-csharp/

        public static I18n instance(object caller) { return IoC.inject.Get<I18n>(caller); }

        public bool isWarningLogEnabled = true;
        public string currentLocale { get; private set; } = "" + EnvironmentV2.instance.CurrentUICulture;
        public CultureInfo currentCulture = EnvironmentV2.instance.CurrentCulture;

        private Func<string, Task<Dictionary<string, Translation>>> localeLoader = LoadLocaleFromFile();
        private Dictionary<string, Translation> translationData;

        public async Task<I18n> SetLocale(string newLocale) {
            currentLocale = newLocale;
            currentCulture = new CultureInfo(currentLocale);
            await LoadTranslationDataForLocale();
            return this;
        }

        public async Task<I18n> SetLocaleLoader(Func<string, Task<Dictionary<string, Translation>>> newLocaleLoader) {
            localeLoader = newLocaleLoader;
            await LoadTranslationDataForLocale();
            return this;
        }

        public async Task<I18n> SetLocaleLoader(Func<string, Task<Dictionary<string, Translation>>> newLocaleLoader, string newLocale) {
            localeLoader = newLocaleLoader;
            currentLocale = newLocale;
            await LoadTranslationDataForLocale();
            return this;
        }

        private async Task LoadTranslationDataForLocale() {
            AssertV2.IsNotNull(localeLoader, "localeLoader");
            translationData = await localeLoader(currentLocale);
        }

        public string Get(string key, params object[] args) {
            string translation = key;
            if (translationData == null) { LoadTranslationDataForLocale().Wait(); }
            if (translationData != null && translationData.TryGetValue(key, out Translation t)) {
                translation = GetTranslation(t, args);
            }
            // check if we have additional embeddable data:
            if (args.Length > 0) { return string.Format(currentCulture, translation, args); }
            return translation;
        }

        private string GetTranslation(Translation t, object[] args) {
            if (t.zero != null || t.one != null) {
                var number = GetNumberInArgs(args);
                if (number == 0) { return t.zero; }
                if (number == 1) { return t.one; }
            }
            if (t.other != null) { return t.other; }
            return t.key;
        }

        private static int GetNumberInArgs(object[] args) {
            var firstNumberFound = args.First(a => IsNumeric(a));
            var argOne = Math.Abs(Convert.ToInt32(firstNumberFound));
            if (argOne == 0 && Math.Abs(Convert.ToDouble(firstNumberFound)) != 0) { return 2; }
            if (argOne == 1 && Math.Abs(Convert.ToDouble(firstNumberFound)) != 1) { return 2; }
            return argOne;
        }

        private static bool IsNumeric(object Expression) {
            if (Expression == null || Expression is DateTime) { return false; }
            return Expression is short || Expression is int || Expression is long
                    || Expression is decimal || Expression is float
                    || Expression is double || Expression is bool;
        }

        public static Func<string, Task<Dictionary<string, Translation>>> LoadLocaleFromFile() {
            return (string localeName) => {
                var dir = EnvironmentV2.instance.GetCurrentDirectory().GetChildDir("Locales");
                var f = dir.GetChild(localeName);
                if (!f.Exists) { f = dir.GetChild(localeName + ".json"); }
                if (f.Exists) { return ConvertToDictionary(f.LoadAs<List<Translation>>()); }
                throw new FileNotFoundException("Can't load localization file: " + localeName);
            };
        }

        public static Func<string, Task<Dictionary<string, Translation>>> LoadLocaleFromGoogleSheets(
                IKeyValueStore localCache, string apiKey, string sheetId, string initialSheetName = "en-US") {
            var translationDatabase = new GoogleSheetsKeyValueStore(localCache, apiKey, sheetId, initialSheetName);
            return async (string localeName) => {
                if (localeName != translationDatabase.sheetName) { translationDatabase.sheetName = localeName; }
                var downloadTask = translationDatabase.dowloadOnlineDataDebounced();
                if (downloadTask != null && !await downloadTask) { // Make sure the data is downloaded at least once
                    Log.w($"Could not download {localeName} translation data from sheet {sheetId}, device offline?");
                }
                return await ConvertToDictionary(await translationDatabase.GetAll<Translation>());
            };
        }

        private static Task<Dictionary<string, Translation>> ConvertToDictionary(IEnumerable<Translation> translations) {
            AssertV2.AreNotEqual(0, translations.Count());
            return Task.FromResult(translations.ToDictionary(e => e.key, e => e));
        }

        public class Translation {
            public string key;
            public string other;
            public string zero;
            public string one;
        }

        public class GSheetsTranslation : Translation {
            public string zeroSrc;
            public string oneSrc;
        }

    }

}