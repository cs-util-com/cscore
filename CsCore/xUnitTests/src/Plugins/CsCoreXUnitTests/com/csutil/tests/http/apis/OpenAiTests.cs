using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis;
using Xunit;

namespace com.csutil.tests.http {

    public class OpenAiTests {

        public OpenAiTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1_TextCompletion() {
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            var prompt = "Complete this sentence with a funny short story: A cow walked ";
            var result = await openAi.Complete(prompt);
            var completion = result.choices.Single().text;
            Assert.NotEmpty(completion);
            Log.d(prompt + completion);
        }

        [Fact]
        public async Task ExampleUsage2_ImageGeneration() {
            // The OpenAi key for DallE 2 currently needs to be grabbed from the "authorization: Bearer ..." header of a
            // test request performed on https://labs.openai.com/ since the DallE 2 service is not yet released with official API access
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            var prompt = "A very cute cat with a cowboy hat in cartoon style";
            var result = await openAi.TextToImage(new OpenAi.Image.Request() { prompt = prompt });
            Assert.NotEmpty(result.data);
            var generatedImageUrls = result.data.Map(x => x.url);
            Assert.NotEmpty(generatedImageUrls);
            Log.d(generatedImageUrls.ToStringV2("", "", " \n\n "));
        }

        [Fact]
        public async Task ExampleUsage3_ChatGpt() {
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            var messages = new List<ChatGpt.Line>() {
                new ChatGpt.Line(ChatGpt.Role.system, content: "You are a sarcastic developer that add puns to all of his responses"),
                new ChatGpt.Line(ChatGpt.Role.user, content: "Do you know cscore, the utility library for C# and Unity?"),
                new ChatGpt.Line(ChatGpt.Role.assistant, content: "Huh? What are you talking about.. just kidding, of course I know it!"),
                new ChatGpt.Line(ChatGpt.Role.user, content: "Any suggestions on interesting features/components to add to that utility library?"),
            };
            var response = await openAi.ChatGpt(new ChatGpt.Request(messages));
            ChatGpt.Line newLine = response.choices.Single().message;
            Assert.Equal("" + ChatGpt.Role.assistant, newLine.role);
            Assert.NotEmpty(newLine.content);

            messages.Add(newLine);
            Log.d("response.content=" + JsonWriter.AsPrettyString(messages));
        }

    }

}