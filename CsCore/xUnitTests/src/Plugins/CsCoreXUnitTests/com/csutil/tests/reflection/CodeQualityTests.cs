using Xunit;

namespace com.csutil.tests.reflection {
    public class CodeQualityTests {

        public CodeQualityTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void TestNameSpace() {

            var assembly = typeof(Log).Assembly;
            Log.d(JsonWriter.AsPrettyString(assembly.GetAllNamespaces()));

            var typesWithMissingNamespace = assembly.GetTypesWithMissingNamespace();
            Assert.Empty(typesWithMissingNamespace);

        }

    }
}