using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace com.csutil.io {

    public class CsvParser {

        public static List<List<string>> ReadCsvStream(Stream csvStream) {
            List<List<string>> rawSheetData = new List<List<string>>();
            using (TextFieldParser csvParser = new TextFieldParser(csvStream, Encoding.Default)) {
                csvParser.TextFieldType = FieldType.Delimited;
                csvParser.SetDelimiters(",");
                while (!csvParser.EndOfData) {
                    string[] row = csvParser.ReadFields();
                    rawSheetData.Add(row.ToList());
                }
            }
            return rawSheetData;
        }

        public static List<JObject> ReadCsvStreamAsJson(Stream csvStream) {
            return ConvertToJson(ReadCsvStream(csvStream));
        }

        public static List<JObject> ConvertToJson(List<List<string>> parsedData) {
            List<JObject> jsonObjects = new List<JObject>();
            if (parsedData.IsNullOrEmpty()) { return jsonObjects; }
            List<string> headers = parsedData[0];
            for (int i = 1; i < parsedData.Count; i++) {
                List<string> row = parsedData[i];
                JObject jsonObject = new JObject();
                for (int j = 0; j < headers.Count; j++) {
                    jsonObject[headers[j]] = j < row.Count ? row[j] : null;
                }
                jsonObjects.Add(jsonObject);
            }
            return jsonObjects;
        }

    }

}