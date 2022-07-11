using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis;
using com.csutil.io;
using Xunit;

namespace com.csutil.tests.http {

    public class OpenAiTests {

        public OpenAiTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            var openAi = new OpenAi(AppSecrets.Load("cscore-secrets-keys.txt")["OpenAiKey"]);
            var result = await openAi.Complete("Complete this funny short story: A cow walked ");
            var answer = result.choices.Single().text;
            Assert.NotEmpty(answer);
            Log.d(answer);
        }

    }

}