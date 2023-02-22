using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace com.csutil.http.apis {

    public static class WttrInApi {

        public static async Task<Response> GetWeather(double latitude, double longitude) {
            var url = new Uri($"https://wttr.in/{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}?M&format=j1");
            return await url.SendGET().GetResult<Response>();
        }

        public class Response {

            public List<Current_conditionItem> current_condition { get; set; }
            public List<Nearest_areaItem> nearest_area { get; set; }
            public List<RequestItem> request { get; set; }
            public List<WeatherItem> weather { get; set; }

            public class Current_conditionItem {
                public string FeelsLikeC { get; set; }
                public string FeelsLikeF { get; set; }
                public string cloudcover { get; set; }
                public string humidity { get; set; }
                public string localObsDateTime { get; set; }
                public string observation_time { get; set; }
                public string precipInches { get; set; }
                public string precipMM { get; set; }
                public string pressure { get; set; }
                public string pressureInches { get; set; }
                public string temp_C { get; set; }
                public string temp_F { get; set; }
                public string uvIndex { get; set; }
                public string visibility { get; set; }
                public string visibilityMiles { get; set; }
                public string weatherCode { get; set; }
                public List<StringItem> weatherDesc { get; set; }
                public List<StringItem> weatherIconUrl { get; set; }
                public string winddir16Point { get; set; }
                public string winddirDegree { get; set; }
                public string windspeedKmph { get; set; }
                public string windspeedMiles { get; set; }
            }

            public class Nearest_areaItem {
                public List<StringItem> areaName { get; set; }
                public List<StringItem> country { get; set; }
                public string latitude { get; set; }
                public string longitude { get; set; }
                public string population { get; set; }
                public List<StringItem> region { get; set; }
                public List<StringItem> weatherUrl { get; set; }
            }

            public class RequestItem {
                public string query { get; set; }
                public string type { get; set; }
            }

            public class AstronomyItem {
                public string moon_illumination { get; set; }
                public string moon_phase { get; set; }
                public string moonrise { get; set; }
                public string moonset { get; set; }
                public string sunrise { get; set; }
                public string sunset { get; set; }
            }

            public class HourlyItem {
                public string DewPointC { get; set; }
                public string DewPointF { get; set; }
                public string FeelsLikeC { get; set; }
                public string FeelsLikeF { get; set; }
                public string HeatIndexC { get; set; }
                public string HeatIndexF { get; set; }
                public string WindChillC { get; set; }
                public string WindChillF { get; set; }
                public string WindGustKmph { get; set; }
                public string WindGustMiles { get; set; }
                public string chanceoffog { get; set; }
                public string chanceoffrost { get; set; }
                public string chanceofhightemp { get; set; }
                public string chanceofovercast { get; set; }
                public string chanceofrain { get; set; }
                public string chanceofremdry { get; set; }
                public string chanceofsnow { get; set; }
                public string chanceofsunshine { get; set; }
                public string chanceofthunder { get; set; }
                public string chanceofwindy { get; set; }
                public string cloudcover { get; set; }
                public string humidity { get; set; }
                public string precipInches { get; set; }
                public string precipMM { get; set; }
                public string pressure { get; set; }
                public string pressureInches { get; set; }
                public string tempC { get; set; }
                public string tempF { get; set; }
                public string time { get; set; }
                public string uvIndex { get; set; }
                public string visibility { get; set; }
                public string visibilityMiles { get; set; }
                public string weatherCode { get; set; }
                public List<StringItem> weatherDesc { get; set; }
                public List<StringItem> weatherIconUrl { get; set; }
                public string winddir16Point { get; set; }
                public string winddirDegree { get; set; }
                public string windspeedKmph { get; set; }
                public string windspeedMiles { get; set; }
            }

            public class WeatherItem {
                public List<AstronomyItem> astronomy { get; set; }
                public string avgtempC { get; set; }
                public string avgtempF { get; set; }
                public string date { get; set; }
                public List<HourlyItem> hourly { get; set; }
                public string maxtempC { get; set; }
                public string maxtempF { get; set; }
                public string mintempC { get; set; }
                public string mintempF { get; set; }
                public string sunHour { get; set; }
                public string totalSnow_cm { get; set; }
                public string uvIndex { get; set; }
            }

            public class StringItem {
                public string value { get; set; }
            }

        }

    }

}