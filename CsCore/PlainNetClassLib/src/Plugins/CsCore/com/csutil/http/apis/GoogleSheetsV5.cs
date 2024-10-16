using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.io;
using com.csutil.json;

namespace com.csutil.http.apis {

    public static class GoogleSheetsV5 {

        public static Task<Dictionary<string, T>> GetSheetObjects<T>(Uri csvUrl) {
            return TaskV2.TryWithExponentialBackoff(async () => {
                return GoogleSheetDataParser.ParseRawSheetData<T>(await DownloadAndParseCsvSheet(csvUrl));
            }, HandleError, maxNrOfRetries: 5);
        }

        public static Task<Dictionary<string, object>> GetSheetObjects(Uri csvUrl) {
            return TaskV2.TryWithExponentialBackoff(async () => {
                return GoogleSheetDataParser.ParseRawSheetData(await DownloadAndParseCsvSheet(csvUrl));
            }, HandleError, maxNrOfRetries: 5, initialExponent: 9, maxDelayInMs: 5000);
        }

        private static void HandleError(Exception e) {
            if (e is FormatException) { return; } // Retry for this type of exception
            if (e is IndexOutOfRangeException) { return; } // Retry for this type of exception
            throw e; // for any other error stop trying 
        }

        private static async Task<List<List<string>>> DownloadAndParseCsvSheet(Uri csvUrl) {
            AssertV3.IsTrue(csvUrl.Query.Contains("output=csv"), () => "The passed url is not a CSV export url from 'File => Publish to the web'");
            using (var csvStream = await csvUrl.SendGET().GetResult<Stream>()) {
                return CsvParser.ReadCsvStream(csvStream);
            }
        }

    }

    public static class GoogleSheetDataParser {

        public static Dictionary<string, T> ParseRawSheetData<T>(List<List<string>> rawSheetData) {
            var result = new Dictionary<string, T>();
            if (rawSheetData.IsNullOrEmpty()) { return result; }
            var fieldNames = rawSheetData.First().ToList();
            foreach (var column in rawSheetData.Skip(1)) {
                var key = column.First();
                AssertV3.AreNotEqual("", key);
                var obj = ToObject(fieldNames, column.ToList());
                result.Add(key, ToTypedObject<T>(obj));
            }
            return result;
        }

        private static T ToTypedObject<T>(object o) {
            var json = JsonWriter.GetWriter().Write(o);
            return TypedJsonHelper.NewTypedJsonReader().Read<T>(json);
        }

        public static Dictionary<string, object> ParseRawSheetData(List<List<string>> rawSheetData) {
            var result = new Dictionary<string, object>();
            if (rawSheetData.IsNullOrEmpty()) { return result; }
            var fieldNames = rawSheetData.First().ToList();
            foreach (var column in rawSheetData.Skip(1)) {
                var key = column.First();
                AssertV3.AreNotEqual("", key);
                result.Add(key, ToObject(fieldNames, column.ToList()));
            }
            return result;
        }

        private static object ToObject(List<string> names, List<string> values) {
            var nc = names.Count();
            var vCount = values.Count();
            if (nc < vCount) { throw new IndexOutOfRangeException($"Only {nc} names but {vCount} values in row. names={names.ToStringV2(x => x)} but values={values.ToStringV2(x => x)}"); }
            var result = new Dictionary<string, object>();
            var jsonReader = JsonReader.GetReader();
            for (int i = 0; i < vCount; i++) { AddToResult(result, jsonReader, names[i], values[i].Trim()); }
            return result;
        }

        private static bool AddToResult(Dictionary<string, object> result, IJsonReader jsonReader, string fieldName, string value) {
            if (value.IsNullOrEmpty()) { return false; }
            try {
                if (value.StartsWith("{") && value.EndsWith("}")) {
                    result.Add(fieldName, jsonReader.Read<Dictionary<string, object>>(value));
                    return true;
                }
                if (value.StartsWith("[") && value.EndsWith("]")) {
                    result.Add(fieldName, jsonReader.Read<List<object>>(value));
                    return true;
                }
            } catch (Exception e) { Log.e(e); }
            result.Add(fieldName, ParsePrimitive(value));
            return true;
        }

        private static object ParsePrimitive(string value) {
            if (bool.TryParse(value, out bool b)) { return b; }
            if (int.TryParse(value, out int i)) { return i; }
            if (DoubleTryParseV2(value, out double d)) { return d; }
            return value;
        }

        private static bool DoubleTryParseV2(string value, out double d) {
            return double.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out d);
        }

    }

}