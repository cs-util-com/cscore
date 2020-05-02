using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace com.csutil {

    public static class UnityLanguageToCultureMapper {

        private static readonly Dictionary<SystemLanguage, string> map = InitLanguageToCodeMapping();

        public static CultureInfo ToCultureInfo(this SystemLanguage language) {
            return CultureInfo.CreateSpecificCulture(map.GetValue(language, "en"));
        }

        private static Dictionary<SystemLanguage, string> InitLanguageToCodeMapping() {
            var map = new Dictionary<SystemLanguage, string>(); // Serializable subclass of normal Dictionary
            map.Add(SystemLanguage.Afrikaans, "af");
            map.Add(SystemLanguage.Arabic, "ar");
            map.Add(SystemLanguage.Basque, "eu");
            map.Add(SystemLanguage.Belarusian, "be");
            map.Add(SystemLanguage.Bulgarian, "bg");
            map.Add(SystemLanguage.Catalan, "ca");
            map.Add(SystemLanguage.Chinese, "zh");
            map.Add(SystemLanguage.Czech, "cs");
            map.Add(SystemLanguage.Danish, "da");
            map.Add(SystemLanguage.Dutch, "nl");
            map.Add(SystemLanguage.English, "en");
            map.Add(SystemLanguage.Estonian, "et");
            map.Add(SystemLanguage.Faroese, "fo");
            map.Add(SystemLanguage.Finnish, "fi");
            map.Add(SystemLanguage.French, "fr");
            map.Add(SystemLanguage.German, "de");
            map.Add(SystemLanguage.Greek, "el");
            map.Add(SystemLanguage.Hebrew, "he");
            map.Add(SystemLanguage.Icelandic, "is");
            map.Add(SystemLanguage.Indonesian, "id");
            map.Add(SystemLanguage.Italian, "it");
            map.Add(SystemLanguage.Japanese, "ja");
            map.Add(SystemLanguage.Korean, "ko");
            map.Add(SystemLanguage.Latvian, "lv");
            map.Add(SystemLanguage.Lithuanian, "lt");
            map.Add(SystemLanguage.Norwegian, "no");
            map.Add(SystemLanguage.Polish, "pl");
            map.Add(SystemLanguage.Portuguese, "pt");
            map.Add(SystemLanguage.Romanian, "ro");
            map.Add(SystemLanguage.Russian, "ru");
            map.Add(SystemLanguage.SerboCroatian, "sr");
            map.Add(SystemLanguage.Slovak, "sk");
            map.Add(SystemLanguage.Slovenian, "ls");
            map.Add(SystemLanguage.Spanish, "es");
            map.Add(SystemLanguage.Swedish, "sv");
            map.Add(SystemLanguage.Thai, "th");
            map.Add(SystemLanguage.Turkish, "tr");
            map.Add(SystemLanguage.Ukrainian, "uk");
            map.Add(SystemLanguage.Vietnamese, "vi");
            map.Add(SystemLanguage.ChineseSimplified, "zh-cn");
            map.Add(SystemLanguage.ChineseTraditional, "zh-tw");
            return map;
        }

    }

}