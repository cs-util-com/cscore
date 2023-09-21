using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis;
using com.csutil.http.apis.iplookup;
using Xunit;

namespace com.csutil.tests.http {
    
    public class WeatherReportExamples {

        public WeatherReportExamples(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task OpenMeteoComExample1() {
            GeoPluginNet.Response response = await GeoPluginNet.GetResponse();
            var latitude = double.Parse(response.geoplugin_latitude, CultureInfo.InvariantCulture);
            var longitude = double.Parse(response.geoplugin_longitude, CultureInfo.InvariantCulture);
            var report = await OpenMeteoCom.GetForecast(latitude, longitude);
            Log.d($"Full weather report for ({latitude},{longitude}): " + JsonWriter.AsPrettyString(report));
            Assert.NotEmpty(report.hourly.temperature_2m);
        }

        [Fact]
        public async Task MetWeatherAPIExample1() {
            GeoPluginNet.Response response = await GeoPluginNet.GetResponse();
            var latitude = double.Parse(response.geoplugin_latitude, CultureInfo.InvariantCulture);
            var longitude = double.Parse(response.geoplugin_longitude, CultureInfo.InvariantCulture);
            var report = await MetWeatherAPI.GetLocationForecast(latitude, longitude, "ExampleUsageDemo github.com/cs-util-com/cscore");
            Log.d($"Full weather report for ({latitude},{longitude}): " + JsonWriter.AsPrettyString(report));
            Assert.NotEmpty(report.properties.timeseries);
        }

        [Fact]
        public async Task WttrInApiExample1() {
            GeoPluginNet.Response response = await GeoPluginNet.GetResponse();
            var latitude = double.Parse(response.geoplugin_latitude, CultureInfo.InvariantCulture);
            var longitude = double.Parse(response.geoplugin_longitude, CultureInfo.InvariantCulture);
            var report = await WttrInApi.GetWeather(latitude, longitude);
            Log.d($"Full weather report for ({latitude},{longitude}): " + JsonWriter.AsPrettyString(report));
            Assert.NotEmpty(report.weather.First().hourly.First().weatherDesc);
        }

        /// <summary> MetaWeather seems to be down for quite a while, will check from time to time if its back  </summary>
        private DateTime MetaWeatherComNextCheckIfBackOnline = new DateTime(2024, 01, 01);

        [Obsolete]
        [Fact]
        public async Task MetaWeatherComExample1() {

            if (DateTimeV2.Now < MetaWeatherComNextCheckIfBackOnline) { return; }

            var ipLookupResult = await IpApiCom.GetResponse();
            string yourCity = ipLookupResult.city;
            var cityLookupResult = await MetaWeatherLocationLookup.GetLocation(yourCity);
            if (cityLookupResult.IsNullOrEmpty()) {
                cityLookupResult = await MetaWeatherLocationLookup.GetLocation((float)ipLookupResult.lat, (float)ipLookupResult.lon);
            }
            Assert.False(cityLookupResult.IsNullOrEmpty(), "Did not find any location for city=" + yourCity);
            int whereOnEarthIDOfYourCity = cityLookupResult.First().woeid;
            var report = await MetaWeatherReport.GetReport(whereOnEarthIDOfYourCity);
            Log.d("Full weather report: " + JsonWriter.AsPrettyString(report));
            var currentWeather = report.consolidated_weather.Map(r => r.weather_state_name);
            Log.d("The weather today in " + yourCity + " is: " + currentWeather.ToStringV2());

        }

        [Obsolete]
        [Fact]
        public async Task MetaWeatherComTest1() {

            if (DateTimeV2.Now < MetaWeatherComNextCheckIfBackOnline) { return; }

            var berlinName = "Berlin";
            float berlinLatitude = 52.50f;
            float berlinLongitude = 13.40f;

            var foundLocations = await MetaWeatherLocationLookup.GetLocation(berlinName);
            var whereOnEarthID = foundLocations.First().woeid;
            var weatherReports1 = await MetaWeatherReport.GetReport(whereOnEarthID);

            Assert.False(berlinLatitude.ToString(CultureInfo.InvariantCulture).Contains(","), "float.toString contains comma, this would break the REST query");
            var foundLocations2 = await MetaWeatherLocationLookup.GetLocation(berlinLatitude, berlinLongitude);
            var whereOnEarthID2 = foundLocations2.First().woeid;
            var weatherReports2 = await MetaWeatherReport.GetReport(whereOnEarthID2);

            Assert.Equal(whereOnEarthID, whereOnEarthID2);

            var summary1 = weatherReports1.consolidated_weather.Map(report => report.weather_state_name);
            var summary2 = weatherReports2.consolidated_weather.Map(report => report.weather_state_name);
            Assert.NotEmpty(summary1);
            Assert.Equal(summary1, summary2);

        }

        [Obsolete]
        [Fact]
        public async Task MetaWeatherComTest2() {

            if (DateTimeV2.Now < MetaWeatherComNextCheckIfBackOnline) { return; }

            var cityName = "Berlin";
            var w = await MetaWeather.GetWeather(cityName);
            var summary1 = w.consolidated_weather.Map(report => report.weather_state_name);
            Assert.NotEmpty(summary1);
        }

    }

}