using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis;
using Xunit;

namespace com.csutil.tests.http {

    public class OpenAiTests {

        public OpenAiTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            var prompt = "Complete this funny short story: A cow walked ";
            var result = await openAi.Complete(prompt);
            var completion = result.choices.Single().text;
            Assert.NotEmpty(completion);
            Log.d(prompt + completion);
        }

    }

}