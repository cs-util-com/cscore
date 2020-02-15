using System;
using System.Threading.Tasks;
using com.csutil.http.apis.iplookup;
using Xunit;

namespace com.csutil.tests.http {

    /// <summary> Some REST API examples and how they would be used in async methods </summary>
    public class IpLookupExamples {

        [Fact]
        public async Task IpifyOrgExample() {
            IpifyOrg.Response response = await IpifyOrg.GetResponse();
            Log.d("Your IP is " + response.ip);
            Assert.NotEmpty(response.ip);
        }

        [Fact]
        public async Task IpApiComExample() {
            IpApiCom.Response response = await IpApiCom.GetResponse();
            Log.d("Your IP is " + response.query);
            Log.d("The name of your city is " + response.city);
            Assert.NotEmpty(response.query);
            Assert.NotEmpty(response.@as);
        }

        [Fact]
        public async Task GeoPluginNetExampleExample() {
            GeoPluginNet.Response response = await GeoPluginNet.GetResponse();
            Log.d("Your IP is " + response.geoplugin_request);
            Log.d("The name of your region is " + response.geoplugin_regionName);
            Assert.NotEmpty(response.geoplugin_request);
        }

        [Fact]
        public async Task ExtremeIpLookupComExample() {
            ExtremeIpLookupCom.Response response = await ExtremeIpLookupCom.GetResponse();
            Log.d("Your IP is " + response.query);
            Log.d("You are running this test from your home: " + ("Residential".Equals(response.ipType)));
            Assert.NotEmpty(response.query);
        }

    }

}