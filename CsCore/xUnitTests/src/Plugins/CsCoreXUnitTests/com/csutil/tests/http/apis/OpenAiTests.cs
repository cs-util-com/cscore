using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis;
using com.csutil.model.jsonschema;
using Xunit;
using com.csutil.http;
using Zio;
using System.IO;

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

        /// <summary> An example of how to use the ChatGpt API to get a response that is automatically parsed as a json object </summary>
        [Fact]
        public async Task ExampleUsage4_ChatGptJsonResponses() {

            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            var messages = new List<ChatGpt.Line>();
            messages.Add(new ChatGpt.Line(ChatGpt.Role.system, content: "You are a helpful assistant designed to output JSON."));

            { // The user inputs a question but the response should be automatically parsable as a YesNoResponse:

                // Create an example object so that the AI knows how the response json should look like for user inputs:
                var yesNoResponseFormat = new YesNoResponse() {
                    confidence = 100,
                    inputQuestionInterpreted = "Is the sky blue?",
                    yesNoAnswer = true,
                    explanation = "The sky is blue because of the way the atmosphere interacts with sunlight."
                };
                messages.AddUserLineWithJsonResultStructure("Can dogs look up?", yesNoResponseFormat);

                // Send the messages to the AI and get the response:
                var response = await openAi.ChatGpt(NewGpt4JsonRequestWithFullConversation(messages));
                ChatGpt.Line newLine = response.choices.Single().message;
                messages.Add(newLine);

                // Parse newLine.content as a YesNoResponse:
                var yesNoResponse = newLine.ParseNewLineContentAsJson<YesNoResponse>();

                // Dogs can look up, lets hope the AI knows that too:
                Assert.True(yesNoResponse.yesNoAnswer);
                // Since the input question is very short the interpretation will be the same string:
                Assert.Equal("Can dogs look up?", yesNoResponse.inputQuestionInterpreted);
                // The AI is very confident in its answer:
                Assert.True(yesNoResponse.confidence > 50);
                // The AI also explains why it gave the answer:
                Assert.NotEmpty(yesNoResponse.explanation);

            }
            {
                // It is possible to not loose the context of the entire previous conversation but still use
                // different response classes to parse into for each additional user question:
                {
                    var userInput = "I hate you, you are the worst AI!";
                    var emotionalResponse = await TalkToEmotionalAi(openAi, messages, userInput);
                    Assert.NotEqual(EmotionalChatResponse.Emotion.happy, emotionalResponse.emotionOfResponse);
                    Assert.NotEmpty(emotionalResponse.aiAnswer);
                }
                {
                    var userInput = "I am sorry, that was just a test of your emotional intelligence. Of course I love you!";
                    var answer = await TalkToEmotionalAi(openAi, messages, userInput);
                    Assert.Equal(EmotionalChatResponse.Emotion.happy, answer.emotionOfResponse);
                    Assert.NotEmpty(answer.aiAnswer);
                }
            }
            // Show the entire conversation to make it clear how the responses look as strings:
            Log.d("messages=" + JsonWriter.AsPrettyString(messages));
        }

        private static async Task<EmotionalChatResponse> TalkToEmotionalAi(OpenAi openAi, List<ChatGpt.Line> messages, string userInput) {
            using var timing = Log.MethodEnteredWith(userInput);
            EmotionalChatResponse emotionalResponseFormat = new EmotionalChatResponse() {
                emotionOfResponse = EmotionalChatResponse.Emotion.happy,
                aiAnswer = "Thanks, that is very nice of you!"
            };
            messages.AddUserLineWithJsonResultStructure(userInput, emotionalResponseFormat);
            var response = await openAi.ChatGpt(NewGpt4JsonRequestWithFullConversation(messages));
            ChatGpt.Line newLine = response.choices.Single().message;
            messages.Add(newLine);

            // Parse newLine.content as a YesNoResponse:
            var emotionalChatResponse = newLine.ParseNewLineContentAsJson<EmotionalChatResponse>();
            return emotionalChatResponse;
        }

        private static ChatGpt.Request NewGpt4JsonRequestWithFullConversation(List<ChatGpt.Line> conversationSoFar) {
            var request = new ChatGpt.Request(conversationSoFar);
            // Use json as the response format:
            request.response_format = ChatGpt.Request.ResponseFormat.json;
            request.model = "gpt-4-1106-preview"; // See https://platform.openai.com/docs/models/gpt-4
            return request;
        }

        public class YesNoResponse {

            [Description("The confidence of the AI in the answer")]
            public int confidence { get; set; }

            [Description("The summary of the input question that the AI used to give the answer")]
            public string inputQuestionInterpreted { get; set; }

            [Description("The yes/no decision of the AI for the input question")]
            public bool yesNoAnswer { get; set; }

            [Description("The explanation of the AI why it gave the answer")]
            public string explanation { get; set; }

        }


        public class EmotionalChatResponse {

            public enum Emotion { happy, sad, angry }

            [Description("How the AI feels about the users question")]
            public Emotion emotionOfResponse { get; set; }

            [Description("The answer/response of the AI to the users input")]
            public string aiAnswer { get; set; }

        }


        [Fact]
        public async Task ExampleTTSandSTT() {
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));

            string textToTest = "hello world";
            var responseTTS = await openAi.TextToSpeech(new OpenAi.Audio.TTSRequest() { input = textToTest });
            Assert.NotNull(responseTTS);

            var responseSTT = await openAi.SpeechToText(new OpenAi.Audio.STTRequest() { fileStream = responseTTS });
            Assert.NotEmpty(responseSTT.text);
            Log.d(responseSTT.text);
            //fails when textToTest contains numbers
            Assert.Equal(formatString(responseSTT.text), formatString(textToTest));
        }

        private string formatString(string str) {
            //helper function to format TTS and STT converted texts to test whether input text and output text is equal
            //remove all characters except alphabets, i.e. white spaces, numbers, special characters
            return new String(str.ToCharArray().Where(c => !Char.IsWhiteSpace(c) && Char.IsLetterOrDigit(c))
            .ToArray()).ToLower();

        }


    }

}