using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using com.csutil;
using Newtonsoft.Json.Linq;

namespace com.csutil.json {

    public static class JsonReaderAssertionExtensions {

        private const int MAX_DEPTH = 10;

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        public static void AssertThatJsonWasFullyParsedIntoFields<T>(this IJsonReader jsonReader, IJsonWriter jsonWriter, string input, T result) {
            if (jsonWriter == null) { return; }
            ValidateIfAllJsonFieldsWereParsedIntoObject(jsonReader, jsonWriter, input, typeof(T), result);
        }

        private static void ValidateIfAllJsonFieldsWereParsedIntoObject(IJsonReader jsonReader, IJsonWriter jsonWriter, string input, Type targetType, object result) {
            try {
                if (String.IsNullOrEmpty(input)) {
                    Log.w("Emtpy input string can't be parsed into TargetType=" + targetType);
                } else if (!targetType.Equals(typeof(System.Collections.Generic.Dictionary<string, object>))
                    && !(targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))) {
                    // do not do the validation for Dictionary<string, object> or generic lists to avoid stack overflow
                    var errorText = "Did not fully parse json=<<((  " + input + "  ))>>";
                    var args = new StackFrame(3, true).AddTo(null);
                    if (!EnvironmentV2.isWebGL) {
                        AssertV2.IsTrue(JsonCouldBeFullyParsed(jsonReader, jsonWriter, result, input), errorText, args);
                    }
                }
            }
            catch (Exception e) { Log.e(e); }
        }

        private static bool JsonCouldBeFullyParsed(IJsonReader jsonReader, IJsonWriter jsonWriter, object result, string json) {
            try {
                json = RemoveNonAsciiCharacters(json);
                AssertV2.IsFalse(string.IsNullOrEmpty(json), "Json isNullOrEmpty");
                var input = jsonReader.Read<Dictionary<string, object>>(json);
                var parsed = jsonReader.Read<Dictionary<string, object>>(jsonWriter.Write(result));
                AssertV2.IsNotNull(parsed, "parsed");
                return JsonCouldBeFullyParsed(jsonReader, result.GetType().Name, input, parsed, 0);
            }
            catch (Exception e) { Log.e(new Exception("exception during parsing json=" + json, e)); }
            return false;
        }

        private static string RemoveNonAsciiCharacters(string input) {
            // From https://stackoverflow.com/a/123340/165106 :
            return Regex.Replace(input, @"[^\u0000-\u007F]+", string.Empty);
        }

        private static bool JsonCouldBeFullyParsed(IJsonReader reader, string path, IDictionary input, IDictionary parsed, int depth) {
            if (depth > MAX_DEPTH) { Log.e("Deth > " + MAX_DEPTH + ", will abort recursive search on this level, path=" + path); return false; }
            foreach (var f in input) {
                var field = (DictionaryEntry)f;
                var key = field.Key;
                var value = field.Value;
                value = JsonReader.convertToGenericDictionaryOrArray(value);
                if (value != null && !parsed.Contains(key)) {
                    var infoStringAboutField = "field " + path + "." + key + " = " + value;
                    if (value != null) { infoStringAboutField += ", value type=(" + value.GetType() + ")"; }
                    var args = new StackFrame(5 + depth, true).AddTo(null);
                    Log.e(" > Missing " + infoStringAboutField, args);
                    return false;
                } else if (value is IDictionary) {
                    var a = value as IDictionary;
                    var valueInParsedDict = JsonReader.convertToGenericDictionaryOrArray(parsed[key]);
                    var b = valueInParsedDict as IDictionary;
                    var args = new StackFrame(5 + depth, true).AddTo(null);
                    AssertV2.IsNotNull(b, "Field was found but it was not a JsonObject, it was a " + valueInParsedDict.GetType(), args);
                    return JsonCouldBeFullyParsed(reader, path + "." + key, a, b, depth + 1);
                } else if (value is IDictionary[]) {
                    var a = value as IDictionary[];
                    var valueInParsedArray = JsonReader.convertToGenericDictionaryOrArray(parsed[key]);
                    var b = valueInParsedArray as IDictionary[];
                    var args = new StackFrame(5 + depth, true).AddTo(null);
                    AssertV2.IsNotNull(b, "Field was found but it was not a JsonArray, it was a " + valueInParsedArray.GetType(), args);
                    AssertV2.AreEqual(a.Length, b.Length, "", args);
                    var r = true;
                    for (int i = 0; i < a.Length; i++) {
                        r = JsonCouldBeFullyParsed(reader, path + "." + key + "[" + i + "]", a[i], b[i], depth + 1) & r;
                    }
                    return r;
                }
            }
            return true;
        }

    }

}