//#define RUN_EXPENSIVE_TESTS
// TODO enable this define to run the expensive tests OpenAI tests as well

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis;
using com.csutil.model.jsonschema;
using Xunit;
using Newtonsoft.Json;
using System.IO;
using Zio;

namespace com.csutil.integrationTests.http {

    public class OpenAiTests {

        public OpenAiTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1_ChatGpt() {
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
        public async Task ExampleUsage2_ChatGpt4() {
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            var messages = new List<ChatGpt.Line>() {
                new ChatGpt.Line(ChatGpt.Role.system, content: "You are a standup comedian. You are on stage and about to tell a joke."),
                new ChatGpt.Line(ChatGpt.Role.user, content: "Do you know the joke about the chicken that crossed the road?"),
                new ChatGpt.Line(ChatGpt.Role.assistant, content: "Yes I actually happen to know the best one of all chicken jokes."),
                new ChatGpt.Line(ChatGpt.Role.user, content: "Why did the chicken cross the road?"),
            };
            var request = new ChatGpt.Request(messages);
            request.model = "gpt-4o"; // See https://platform.openai.com/docs/models/gpt-4
            var response = await openAi.ChatGpt(request);
            ChatGpt.Line newLine = response.choices.Single().message;
            Assert.Equal("" + ChatGpt.Role.assistant, newLine.role);
            Assert.NotEmpty(newLine.content);

            messages.Add(newLine);
            Log.d("response.content=" + JsonWriter.AsPrettyString(messages));
        }

        /// <summary> An example of how to use the ChatGpt API to get a response that is automatically parsed as a json object </summary>
        [Fact]
        public async Task ExampleUsage3_ChatGptJsonResponses() {

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
                Assert.Contains("dog", yesNoResponse.inputQuestionInterpreted);
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

        #if RUN_EXPENSIVE_TESTS
        [Fact]
        #endif
        public async Task ExampleUsage5_TextToImage() {
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            var prompt = "A cute cat with a cowboy hat in cartoon style";
            var result = await openAi.TextToImage(new OpenAi.Image.Request() { prompt = prompt });
            Assert.NotEmpty(result.data);
            var generatedImageUrls = result.data.Map(x => x.url);
            Assert.NotEmpty(generatedImageUrls);
            Log.d(generatedImageUrls.ToStringV2("", "", " \n\n "));
        }

        #if RUN_EXPENSIVE_TESTS
        [Fact]
        #endif
        public async Task ExampleUsage6_ImageGenerationHighQuality() {
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

        /// <summary> Shows how to use the OpenAI API to generate multiple images in parallel using the
        /// DALL-E 3 model with different randomized input parameters </summary>
        #if RUN_EXPENSIVE_TESTS
        [Fact]
        #endif
        public async Task ExampleUsage6_BulkImageGeneration() {
            var parallelImgGenTaskCount = 2; // Up to 7 should be possible in the first API tier, later you are allowed to go higher
            var targetFolder = EnvironmentV2.instance.GetOrAddTempFolder("cscore BULK IMAGE GENERATION");
            var prompt = "A cute cat with a cowboy hat";
            var tasks = new List<Task>();
            // For image gen. rate limits per minute see https://platform.openai.com/docs/guides/rate-limits/usage-tiers 
            for (int i = 0; i < parallelImgGenTaskCount; i++) {
                tasks.Add(GenerateAndSaveDalle3Image(prompt, targetFolder));
            }
            await Task.WhenAll(tasks);
        }

        private static async Task GenerateAndSaveDalle3Image(string prompt, DirectoryEntry folderToSaveTo) {
            var rnd = new Random();
            var request = new OpenAi.Image.Request() {
                prompt = prompt,
                model = "dall-e-3",
                quality = rnd.ShuffleEntries(new[] { "standard", "hd" }).First(),
                style = rnd.ShuffleEntries(new[] { "natural", "vivid" }).First()
            };
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            var result = await openAi.TextToImage(request);
            Assert.NotEmpty(result.data);
            var generatedImageUrls = result.data.Map(x => new Uri(x.url));
            foreach (var url in generatedImageUrls) {
                Log.d("url=" + url);
                var targetFile = folderToSaveTo.GetChild($"{request.style} {request.quality} image {DateTimeV2.Now.ToLocalUiStringV2()} {url.Segments.Last()}");
                Stream imageStream = await url.SendGET().GetResult<Stream>();
                imageStream = await imageStream.CopyToSeekableStreamIfNeeded(true);
                await targetFile.SaveStreamAsync(imageStream);
                imageStream.Dispose();
                Log.d("Downloaded " + targetFile.GetFullFileSystemPath());
            }
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

        #if RUN_EXPENSIVE_TESTS
        [Fact]
        #endif
        public async Task ExampleUsage6_ImageToText() {
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));

            var prompt = "A picture of a dog";
            var result = await openAi.TextToImage(new OpenAi.Image.Request() { prompt = prompt });
            var url = result.data.First().url;
            Assert.NotEmpty(url);

            var messages = new List<VisionGpt.Line>() {
                new VisionGpt.Line(ChatGpt.Role.system, content: "You are a helpful assistant designed to output JSON.")
            };

            var yesNoResponseFormat = new YesNoResponse() {
                confidence = 100,
                inputQuestionInterpreted = "Is there a cat in the image?",
                yesNoAnswer = true,
                explanation = "The cat is in the picture because I see a small feline with whiskers."
            };
            messages.AddImageURL(url);
            messages.AddUserLineWithJsonResultStructure("Is there a dog in the picture?", yesNoResponseFormat);

            // Send the messages to the AI and get the response:
            var response = await openAi.ImageToText(new VisionGpt.Request(messages));
            VisionGpt.Line newLine = response.choices.Single().message;
            messages.Add(newLine);

            // Parse newLine.content as a YesNoResponse:
            var yesNoResponse = newLine.ParseNewLineContentAsJson<List<YesNoResponse>>().Single();

            // Dogs can look up, lets hope the AI knows that too:
            Assert.True(yesNoResponse.yesNoAnswer);
            // The AI is very confident in its answer:
            Assert.True(yesNoResponse.confidence > 50);
            // The AI also explains why it gave the answer:
            Assert.NotEmpty(yesNoResponse.explanation);
            // Show the entire conversation to make it clear how the responses look as strings:
            Log.d("messages=" + JsonWriter.AsPrettyString(messages));
        }

        #if RUN_EXPENSIVE_TESTS
        [Fact]
        #endif
        public async Task ExampleUsage7_AnalyseImage() {

            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));
            int iterationThreshold = 5;
            List<string> imageUrls = new List<string>();

            // Generate images and ask questions about them until we are confident that the image fits the prompt
            for (int iteration = 0; iteration < iterationThreshold; iteration++) {
                var prompt = "A fascinating image from a children's storybook";

                var url = await GenerateImage(openAi, prompt);
                imageUrls.Add(url);
                Assert.NotEmpty(url);

                List<string> questions = await GenerateQuestionsBasedOnPrompt(openAi, prompt);

                // Check that Ai gave back at least one question
                Assert.NotEmpty(questions);

                var confidenceResponseFormat = new ConfidenceResponse() {
                    responseConfidences = new Dictionary<string, int> {
                        { "Does the image evoke a sense of wonder and imagination suitable for a children's storybook?", 100 },
                        { "Are the colors vibrant and appealing to a younger audience?", 70 },
                        { "Does the input image show a picture of a children's book?", 100 },
                        { "Is there a dog in the picture?", 5 }
                    }
                };

                var messages = new List<VisionGpt.Line>() {
                    new VisionGpt.Line(ChatGpt.Role.system, content: "You are a helpful assistant designed to output JSON.")
                };

                messages.AddQuestionsToImage(url, questions);
                messages.AddUserLineWithJsonResultStructure("Rate the following questions with a confidence from 0 to 100 based on how well the question fits the image", confidenceResponseFormat);

                // Send the messages to the AI and get the response:
                var response = await openAi.ImageToText(new VisionGpt.Request(messages));
                VisionGpt.Line newLine = response.choices.Single().message;
                messages.Add(newLine);

                // Parse newLine.content as a YesNoResponse:
                var confidencesResponse = newLine.ParseNewLineContentAsJson<List<ConfidenceResponse>>().Single();

                var isConfident = IsCofidenceHighEnough(confidencesResponse.responseConfidences.Values.ToList());
                if (isConfident) {
                    break;
                }

            }
            string result = JsonConvert.SerializeObject(imageUrls);
            File.WriteAllText(@".\VisionImages.json", result);
        }

        #if RUN_EXPENSIVE_TESTS
        [Fact]
        #endif
        public async Task ExampleUsage8_TextToSpeechAndSpeechToText() {
            var openAi = new OpenAi(await IoC.inject.GetAppSecrets().GetSecret("OpenAiKey"));

            string textToTest = "hello world";
            var responseTTS = await openAi.TextToSpeech(new OpenAi.Audio.TTSRequest() { input = textToTest });
            Assert.NotNull(responseTTS);

            var responseSTT = await openAi.SpeechToText(new OpenAi.Audio.STTRequest() { fileStream = responseTTS });
            Assert.NotEmpty(responseSTT.text);
            Log.d(responseSTT.text);

            string[] split = responseSTT.text.ToLower().Split(new Char[] { ',', '\\', '\n', ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            // Both words (Hello & world) should be in the response:
            Assert.True(split.All(word => textToTest.Contains(new string(word.Where(c => Char.IsLetter(c)).ToArray()))));
        }

        private static async Task<string> GenerateImage(OpenAi openAi, string prompt) {
            var result = await openAi.TextToImage(new OpenAi.Image.Request() { prompt = prompt });
            return result.data.First().url;
        }

        private static async Task<List<string>> GenerateQuestionsBasedOnPrompt(OpenAi openAi, string prompt, int numberOfQuestions = 5) {
            var messages = new List<ChatGpt.Line>();
            messages.Add(new ChatGpt.Line(ChatGpt.Role.system, content: "You are a helpful assistant designed to output JSON."));

            var questionsResponseFormat = new QuestionsResponse() {
                questions = new List<string> {
                    "Does the image evoke a sense of wonder and imagination suitable for a children's storybook?",
                    "Are the colors vibrant and appealing to a younger audience?",
                    "Does the input image show a picture of a children's book?"
                }
            };

            string requestPrompt = "Generate a list of " + numberOfQuestions + " good questions that allow based on the users input prompt '{" + prompt
                + "}' and an image generated by an AI to evaluate if the AI generated image fits well with the user prompt";

            messages.AddUserLineWithJsonResultStructure(requestPrompt, questionsResponseFormat);

            // Send the messages to the AI and get the response:
            var response = await openAi.ChatGpt(NewGpt4JsonRequestWithFullConversation(messages));
            ChatGpt.Line newLine = response.choices.Single().message;
            messages.Add(newLine);

            // Parse newLine.content as a QuestionsResponse:
            var questionsResponse = newLine.ParseNewLineContentAsJson<QuestionsResponse>();
            return questionsResponse.questions;
        }

        private Boolean IsCofidenceHighEnough(List<int> confidences, int threshhold = 50) {
            foreach (int i in confidences) {
                if (i < threshhold) {
                    return false;
                }
            }
            return true;
        }

        private static ChatGpt.Request NewGpt4JsonRequestWithFullConversation(List<ChatGpt.Line> conversationSoFar) {
            var request = new ChatGpt.Request(conversationSoFar);
            // Use json as the response format:
            request.response_format = ChatGpt.Request.ResponseFormat.json;
            request.model = "gpt-4-1106-preview"; // See https://platform.openai.com/docs/models/gpt-4
            return request;
        }

        public class QuestionsResponse {

            [Description("Questions to ask VisionGpt")]
            public List<string> questions { get; set; }
        }

        public class ConfidenceResponse {
            [Description("Confidence for each Response")]
            public Dictionary<string, int> responseConfidences { get; set; }
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

    }

}