using com.csutil.http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public static class RestRequestHelper {

        public static RestRequest WithJsonContent(this RestRequest self, object jsonContent) {
            return self.WithJsonContent(JsonWriter.GetWriter().Write(jsonContent));
        }

        public static RestRequest WithJsonContent(this RestRequest self, string jsonContent) {
            return self.WithTextContent(jsonContent, Encoding.UTF8, "application/json");
        }

        public static string ToUriEncodedString(object o) {
            var map = JsonReader.GetReader().Read<Dictionary<string, object>>(JsonWriter.GetWriter().Write(o));
            return map.Select((x) => x.Key + "=" + Uri.EscapeDataString("" + x.Value)).Aggregate((a, b) => a + "&" + b);
        }

    }

}
