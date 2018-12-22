using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using com.csutil;
using Newtonsoft.Json.Linq;

namespace com.csutil.json {
    public static class JsonReaderAssertionExtensions {

        private const int MAX_DEPTH = 10;

        [Conditional("DEBUG")]
        public static void assertThatJsonWasFullyParsedIntoFields<T>(this IJsonReader jsonReader, IJsonWriter jsonWriter, string input, T result) {
            if (jsonWriter == null) { return; }
            validateIfAllJsonFieldsWereParsedIntoObject(jsonReader, jsonWriter, input, typeof(T), result);
        }

        private static void validateIfAllJsonFieldsWereParsedIntoObject(IJsonReader jsonReader, IJsonWriter jsonWriter, string input, Type targetType, object result) {
            try {
                if (String.IsNullOrEmpty(input)) {
                    Log.w("Emtpy input string can't be parsed into TargetType=" + targetType);
                } else if (!targetType.Equals(typeof(System.Collections.Generic.Dictionary<string, object>))
                    && !(targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))) {
                    // do not do the validation for Dictionary<string, object> or generic lists to avoid stack overflow
                    var errorText = "Did not fully parse json=<<((" + input + "))>>";
                    AssertV2.IsTrue(jsonCouldBeFullyParsed(jsonReader, jsonWriter, result, input), errorText);
                }
            } catch (Exception e) { Log.e(e); }
        }

        private static bool jsonCouldBeFullyParsed(IJsonReader jsonReader, IJsonWriter jsonWriter, object result, string json) {
            try {
                AssertV2.IsFalse(string.IsNullOrEmpty(json), "Json isNullOrEmpty");
                var input = jsonReader.Read<System.Collections.Generic.Dictionary<string, object>>(json);
                //Log.w("input=" + input.ToStringV2());
                var parsed = jsonReader.Read<System.Collections.Generic.Dictionary<string, object>>(jsonWriter.Write(result));
                //Log.w("parsed=" + parsed.ToStringV2());
                return jsonCouldBeFullyParsed(jsonReader, result.GetType().Name, input, parsed, 0);
            } catch (Exception e) {
                Log.e(new Exception("exception during parsing json=" + json, e));
                return false;
            }
        }

        private static bool jsonCouldBeFullyParsed(IJsonReader reader, string path, IDictionary input, IDictionary parsed, int depth) {
            if (depth > MAX_DEPTH) { Log.e("Deth > " + MAX_DEPTH + ", will abort recursive search on this level, path=" + path); return false; }
            foreach (var f in input) {
                var field = (DictionaryEntry)f;
                var key = field.Key;
                var value = field.Value;
                //Assert.IsTrue(field != null);
                value = JsonReader.convertToGenericDictionaryOrArray(value);
                if (!parsed.Contains(key) && value != null) {
                    var infoStringAboutField = "field " + path + "." + key + " = " + value;
                    if (value != null) { infoStringAboutField += ", value type=(" + value.GetType() + ")"; }
                    Log.e(" > Missing " + infoStringAboutField);
                    return false;
                } else if (value is IDictionary) {
                    var a = value as IDictionary;
                    var valueInParsedDict = JsonReader.convertToGenericDictionaryOrArray(parsed[key]);
                    var b = valueInParsedDict as IDictionary;
                    AssertV2.IsNotNull(b, "Field was found but it was not a JsonObject, it was a " + valueInParsedDict.GetType());
                    return jsonCouldBeFullyParsed(reader, path + "." + key, a, b, depth + 1);
                } else if (value is IDictionary[]) {
                    var a = value as IDictionary[];
                    var valueInParsedArray = JsonReader.convertToGenericDictionaryOrArray(parsed[key]);
                    var b = valueInParsedArray as IDictionary[];
                    AssertV2.IsNotNull(b, "Field was found but it was not a JsonArray, it was a " + valueInParsedArray.GetType());
                    AssertV2.AreEqual(a.Length, b.Length);
                    var r = true;
                    for (int i = 0; i < a.Length; i++) {
                        r &= jsonCouldBeFullyParsed(reader, path + "." + key + "[" + i + "]", a[i], b[i], depth + 1);
                    }
                    return r;
                }
            }
            return true;
        }

    }
}