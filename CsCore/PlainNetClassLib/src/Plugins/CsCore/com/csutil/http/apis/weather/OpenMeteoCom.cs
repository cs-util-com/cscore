using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace com.csutil.http.apis {

    public static class OpenMeteoCom {

        public static Task<Response> GetForecast(double latitude, double longitude, bool includeCurrentWeather = true, bool includeNextHours = true, bool includeNextDays = true) {
            // https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&current_weather=true&hourly=temperature_2m,relativehumidity_2m,windspeed_10m,winddirection_10m,cloudcover,snowfall,snow_depth,rain,showers,visibility&daily=apparent_temperature_min,apparent_temperature_max,rain_sum,showers_sum,snowfall_sum,precipitation_hours&timezone=Europe/Berlin
            var queryParams = new Dictionary<string, object>() {
                { "latitude", latitude.ToString(CultureInfo.InvariantCulture) },
                { "longitude", longitude.ToString(CultureInfo.InvariantCulture) },
                { "current_weather", includeCurrentWeather },
                { "timezone", "auto" }
            };
            if (includeNextHours) {
                queryParams.Add("hourly", "temperature_2m,relativehumidity_2m,windspeed_10m,winddirection_10m,cloudcover,snowfall,snow_depth,rain,showers,visibility");
            }
            if (includeNextDays) {
                queryParams.Add("daily", "apparent_temperature_min,apparent_temperature_max,rain_sum,showers_sum,snowfall_sum,precipitation_hours");
            }
            return GetForecast(queryParams);
        }

        public static Task<Response> GetForecast(Dictionary<string, object> queryParams) {
            var uri = new Uri("https://api.open-meteo.com/v1/forecast").WithAddedQueryParams(queryParams);
            // OpenMeteoCom cant handle escaped commas correctly so the escaping that HttpUtility.ParseQueryString does needs to be undone:
            uri = new Uri(Uri.UnescapeDataString(uri.ToString()));
            return uri.SendGET().GetResult<Response>();
        }

        public class Response {

            public double latitude { get; set; }
            public double longitude { get; set; }
            public double generationtime_ms { get; set; }
            public int utc_offset_seconds { get; set; }
            public string timezone { get; set; }
            public string timezone_abbreviation { get; set; }
            public double elevation { get; set; }
            public CurrentWeather current_weather { get; set; }
            public HourlyUnits hourly_units { get; set; }
            public Hourly hourly { get; set; }
            public DailyUnits daily_units { get; set; }
            public Daily daily { get; set; }

            public class CurrentWeather {
                public double temperature { get; set; }
                public double windspeed { get; set; }
                public double winddirection { get; set; }
                public int weathercode { get; set; }
                public string time { get; set; }
            }

            public class Daily {
                public List<string> time { get; set; }
                public List<double> apparent_temperature_min { get; set; }
                public List<double> apparent_temperature_max { get; set; }
                public List<double> rain_sum { get; set; }
                public List<double> showers_sum { get; set; }
                public List<double> snowfall_sum { get; set; }
                public List<double> precipitation_hours { get; set; }
            }

            public class DailyUnits {
                public string time { get; set; }
                public string apparent_temperature_min { get; set; }
                public string apparent_temperature_max { get; set; }
                public string rain_sum { get; set; }
                public string showers_sum { get; set; }
                public string snowfall_sum { get; set; }
                public string precipitation_hours { get; set; }
            }

            public class Hourly {
                public List<string> time { get; set; }
                public List<double> temperature_2m { get; set; }
                public List<int> relativehumidity_2m { get; set; }
                public List<double> windspeed_10m { get; set; }
                public List<int> winddirection_10m { get; set; }
                public List<int> cloudcover { get; set; }
                public List<double> snowfall { get; set; }
                public List<double> snow_depth { get; set; }
                public List<double> rain { get; set; }
                public List<double> showers { get; set; }
                public List<double> visibility { get; set; }
            }

            public class HourlyUnits {
                public string time { get; set; }
                public string temperature_2m { get; set; }
                public string relativehumidity_2m { get; set; }
                public string windspeed_10m { get; set; }
                public string winddirection_10m { get; set; }
                public string cloudcover { get; set; }
                public string snowfall { get; set; }
                public string snow_depth { get; set; }
                public string rain { get; set; }
                public string showers { get; set; }
                public string visibility { get; set; }
            }

        }

    }

}