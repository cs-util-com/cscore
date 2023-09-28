using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.http;
using com.csutil.http.apis;
using Xunit;

namespace com.csutil.integrationTests.http {

    public class XmlParsingTests {

        public XmlParsingTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            var searchTerm = "Star Wars";
            HashSet<string> all = await GoogleSearchSuggestions.GetAlternativesRecursively(searchTerm);
            Log.d(JsonWriter.AsPrettyString(all));

        }

        [Fact]
        public async Task ExampleUsage2() {
            // Google alerts in their settings can be set up to deliver to an RSS feed
            // The RSS feed is in Atom XML format, so it can be parsed like this:
            var atomUrl = new Uri("https://www.google.com/alerts/feeds/18099823142157100289/13332820999335282466");
            var googleAlertsRssFeed = await AtomFeedXml.Get(atomUrl);
            Log.d(JsonWriter.AsPrettyString(googleAlertsRssFeed));
        }

    }

}