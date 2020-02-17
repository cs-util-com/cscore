using System;
using System.Threading.Tasks;

namespace com.csutil.http.apis.iplookup {
    public static class GeoBytesCom {

        public static Task<Response> GetResponse() {
            return new Uri("http://getcitydetails.geobytes.com/GetCityDetails").SendGET().GetResult<Response>();
        }

        public class Response { // generated via http://json2csharp.com :
            public string geobytesforwarderfor { get; set; }
            public string geobytesremoteip { get; set; }
            public string geobytesipaddress { get; set; }
            public string geobytescertainty { get; set; }
            public string geobytesinternet { get; set; }
            public string geobytescountry { get; set; }
            public string geobytesregionlocationcode { get; set; }
            public string geobytesregion { get; set; }
            public string geobytescode { get; set; }
            public string geobyteslocationcode { get; set; }
            public string geobytesdma { get; set; }
            public string geobytescity { get; set; }
            public string geobytescityid { get; set; }
            public string geobytesfqcn { get; set; }
            public string geobyteslatitude { get; set; }
            public string geobyteslongitude { get; set; }
            public string geobytescapital { get; set; }
            public string geobytestimezone { get; set; }
            public string geobytesnationalitysingular { get; set; }
            public string geobytespopulation { get; set; }
            public string geobytesnationalityplural { get; set; }
            public string geobytesmapreference { get; set; }
            public string geobytescurrency { get; set; }
            public string geobytescurrencycode { get; set; }
            public string geobytestitle { get; set; }
        }

    }
}