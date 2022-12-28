using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        
    }
    
}