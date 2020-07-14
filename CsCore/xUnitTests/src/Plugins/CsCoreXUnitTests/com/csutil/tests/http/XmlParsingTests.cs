using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.http.apis;
using Xunit;

namespace com.csutil.tests.http {

    public class XmlParsingTests {

        public XmlParsingTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            var searchTerm = "Star Wars";
            HashSet<string> all = await GoogleSearchSuggestions.GetAlternativesRecursively(searchTerm);
            Log.d(JsonWriter.AsPrettyString(all));

        }

    }

}