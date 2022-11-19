﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.io;

namespace com.csutil.http.apis {

    public static class GoogleSheetsV5 {

        public static async Task<Dictionary<string, object>> GetSheetObjects(Uri csvUrl) {
            return GoogleSheetDataParser.ParseRawSheetData(await GetSheet(csvUrl));
        }

        public static async Task<List<List<string>>> GetSheet(Uri csvUrl) {
            AssertV2.IsTrue(csvUrl.Query.Contains("output=csv"), "The passed url is not a CSV export url from 'File => Publish to the web'");
            var csvStream = await csvUrl.SendGET().GetResult<Stream>();
            return CsvParser.ReadCsvStream(csvStream);
        }

    }

    public static class GoogleSheetDataParser {

        public static Dictionary<string, object> ParseRawSheetData(List<List<string>> rawSheetData) {
            var result = new Dictionary<string, object>();
            if (rawSheetData.IsNullOrEmpty()) { return result; }
            var fieldNames = rawSheetData.First().ToList();
            foreach (var column in rawSheetData.Skip(1)) {
                var key = column.First();
                AssertV2.AreNotEqual("", key);
                result.Add(key, ToObject(fieldNames, column.ToList()));
            }
            return result;
        }

        private static object ToObject(List<string> names, List<string> values) {
            var nc = names.Count();
            var vCount = values.Count();
            if (nc < vCount) { throw new IndexOutOfRangeException($"Only {nc} names but {vCount} values in row"); }
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