using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace com.csutil.http.apis {

    public static class MetaWeather {

        public static async Task<MetaWeatherReport.Report> GetWeather(string cityName) {
            var foundLocations = await MetaWeatherLocationLookup.GetLocation(cityName);
            var whereOnEarthID = foundLocations.First().woeid;
            return await MetaWeatherReport.GetReport(whereOnEarthID);
        }

    }

    public static class MetaWeatherLocationLookup {

        public static Task<List<LocationResp>> GetLocation(float latitude, float longiude) {
            var la = latitude.ToString(CultureInfo.InvariantCulture);
            var lo = longiude.ToString(CultureInfo.InvariantCulture);
            return new Uri("https://www.metaweather.com/api/location/search/?lattlong=" + la + "," + lo).SendGET().GetResult<List<LocationResp>>();
        }

        public static Task<List<LocationResp>> GetLocation(string locationName) {
            return new Uri("https://www.metaweather.com/api/location/search/?query=" + locationName).SendGET().GetResult<List<LocationResp>>();
        }

        public class LocationResp {
            public string title { get; set; }

            public string location_type { get; set; }

            /// <summary> Where On Earth ID - see http://developer.yahoo.com/geo/geoplanet/guide/concepts.html </summary>
            public int woeid { get; set; }

            public string latt_long { get; set; }
        }

    }

    public static class MetaWeatherReport {

        public static Task<Report> GetReport(int woeid) {
            return new Uri("https://www.metaweather.com/api/location/" + woeid).SendGET().GetResult<Report>();
        }

        public class ConsolidatedWeather {
            public object id { get; set; }
            public string weather_state_name { get; set; }
            public string weather_state_abbr { get; set; }
            public string wind_direction_compass { get; set; }
            public DateTime created { get; set; }
            public string applicable_date { get; set; }
            public double min_temp { get; set; }
            public double max_temp { get; set; }
            public double the_temp { get; set; }
            public double wind_speed { get; set; }
            public double wind_direction { get; set; }
            public double air_pressure { get; set; }
            public int humidity { get; set; }
            public double visibility { get; set; }
            public int predictability { get; set; }
        }

        public class WeatherRepSource {
            public string title { get; set; }
            public string slug { get; set; }
            public string url { get; set; }
            public int crawl_rate { get; set; }
        }

        public class Report {
            public List<ConsolidatedWeather> consolidated_weather { get; set; }
            public DateTime time { get; set; }
            public DateTime sun_rise { get; set; }
            public DateTime sun_set { get; set; }
            public string timezone_name { get; set; }
            public Report parent { get; set; }
            public List<WeatherRepSource> sources { get; set; }
            public string title { get; set; }
            public string location_type { get; set; }
            public int woeid { get; set; }
            public string latt_long { get; set; }
            public string timezone { get; set; }
        }

    }

}