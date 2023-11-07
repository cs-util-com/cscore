using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis;
using Xunit;

namespace com.csutil.integrationTests.http {

    public class OpenAiTests {

        public OpenAiTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Obsolete("The .Complete API is deprecated, use .ChatGpt(..) instead")]
        [Fact]
        public async Task ExampleUsage1_TextCompletion() {
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            var prompt = "Complete this sentence with a funny short story: A cow walked ";
            var result = await openAi.Complete(prompt);
            var completion = result.choices.Single().text;
            Assert.NotEmpty(completion);
            Log.d(prompt + completion);
        }

        //[Fact]
        public async Task ExampleUsage2_ImageGeneration() {
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            var prompt = "A cute cat with a cowboy hat in cartoon style";
            var result = await openAi.TextToImage(new OpenAi.Image.Request() { prompt = prompt });
            Assert.NotEmpty(result.data);
            var generatedImageUrls = result.data.Map(x => x.url);
            Assert.NotEmpty(generatedImageUrls);
            Log.d(generatedImageUrls.ToStringV2("", "", " \n\n "));
        }
        
        //[Fact]
        public async Task ExampleUsage2_ImageGeneration2() {
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            var prompt = "A cute cat with a cowboy hat";
            var result = await openAi.TextToImage(new OpenAi.Image.Request() {
                prompt = prompt,
                model = "dall-e-3",
                quality = "hd", 
                style = "natural"
            });
            Assert.NotEmpty(result.data);
            var generatedImageUrls = result.data.Map(x => x.url);
            Assert.NotEmpty(generatedImageUrls);
            Log.d(generatedImageUrls.ToStringV2("", "", " \n\n "));
        }

        [Fact]
        public async Task ExampleUsage3_ChatGpt() {
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            var messages = new List<ChatGpt.Line>() {
                new ChatGpt.Line(ChatGpt.Role.system, content: "You are a standup comedian. You are on stage and about to tell a joke."),
                new ChatGpt.Line(ChatGpt.Role.user, content: "Do you know the joke about the chicken that crossed the road?"),
                new ChatGpt.Line(ChatGpt.Role.assistant, content: "Yes I actually happen to know the best one of all chicken jokes."),
                new ChatGpt.Line(ChatGpt.Role.user, content: "Why did the chicken cross the road?"),
            };
            var response = await openAi.ChatGpt(new ChatGpt.Request(messages));
            ChatGpt.Line newLine = response.choices.Single().message;
            Assert.Equal("" + ChatGpt.Role.assistant, newLine.role);
            Assert.NotEmpty(newLine.content);

            messages.Add(newLine);
            Log.d("response.content=" + JsonWriter.AsPrettyString(messages));
        }

        [Fact]
        public async Task ExampleUsage4_ChatGpt4() {
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            var messages = new List<ChatGpt.Line>() {
                new ChatGpt.Line(ChatGpt.Role.system, content: "You are a standup comedian. You are on stage and about to tell a joke."),
                new ChatGpt.Line(ChatGpt.Role.user, content: "Do you know the joke about the chicken that crossed the road?"),
                new ChatGpt.Line(ChatGpt.Role.assistant, content: "Yes I actually happen to know the best one of all chicken jokes."),
                new ChatGpt.Line(ChatGpt.Role.user, content: "Why did the chicken cross the road?"),
            };
            var request = new ChatGpt.Request(messages);
            request.model = "gpt-4-1106-preview"; // See https://platform.openai.com/docs/models/gpt-4
            var response = await openAi.ChatGpt(request);
            ChatGpt.Line newLine = response.choices.Single().message;
            Assert.Equal("" + ChatGpt.Role.assistant, newLine.role);
            Assert.NotEmpty(newLine.content);

            messages.Add(newLine);
            Log.d("response.content=" + JsonWriter.AsPrettyString(messages));
        }
        
    }

}