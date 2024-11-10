using System;
using System.Threading.Tasks;

namespace com.csutil.http.apis.iplookup {
    public static class GeoPluginNet {

        public static async Task<Response> GetResponse() {
            var response = await new Uri("http://www.geoplugin.net/json.gp").SendGET().GetResult<Response>();
            AssertV3.IsFalse(response.geoplugin_city.IsNullOrEmpty(), () => "geoplugin_city is missing in response");
            return response;
        }

        public class Response { // generated via https://json2csharp.com :
            public string geoplugin_request { get; set; }
            public int geoplugin_status { get; set; }
            public string geoplugin_delay { get; set; }
            public string geoplugin_credit { get; set; }
            
            /// <summary> Can be null, use geoplugin_latitude and geoplugin_longitude as fallback </summary>
            public string geoplugin_city { get; set; }
            
            public string geoplugin_region { get; set; }
            public string geoplugin_regionCode { get; set; }
            public string geoplugin_regionName { get; set; }
            
            /// <summary> Can be null, use geoplugin_latitude and geoplugin_longitude as fallback </summary>
            public string geoplugin_areaCode { get; set; }
            
            public string geoplugin_dmaCode { get; set; }
            public string geoplugin_countryCode { get; set; }
            public string geoplugin_countryName { get; set; }
            public int geoplugin_inEU { get; set; }
            public int geoplugin_euVATrate { get; set; }
            public string geoplugin_continentCode { get; set; }
            public string geoplugin_continentName { get; set; }
            public string geoplugin_latitude { get; set; }
            public string geoplugin_longitude { get; set; }
            public string geoplugin_locationAccuracyRadius { get; set; }
            public string geoplugin_timezone { get; set; }
            public string geoplugin_currencyCode { get; set; }
            public string geoplugin_currencySymbol { get; set; }
            public string geoplugin_currencySymbol_UTF8 { get; set; }
            public double geoplugin_currencyConverter { get; set; }
        }

    }
}