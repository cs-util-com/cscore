using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using com.csutil.model.jsonschema;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using Zio;

namespace com.csutil.http.apis {

    public class OpenAi {

        private string apiKey;

        /// <summary> </summary>
        /// <param name="apiKey"> See https://beta.openai.com/docs/api-reference/authentication and https://beta.openai.com/account/api-keys </param>
        public OpenAi(string apiKey) { this.apiKey = apiKey; }

        /// <summary> https://beta.openai.com/docs/api-reference/images/create </summary>
        public Task<Image.Response> TextToImage(Image.Request requestParams) {
            var request = new Uri("https://api.openai.com/v1/images/generations").SendPOST();
            return request.WithAuthorization(apiKey).WithJsonContent(requestParams).GetResult<Image.Response>();
        }

        /// <summary> https://beta.openai.com/docs/api-reference/images/create </summary>
        public Task<VisionGpt.Response> ImageToText(VisionGpt.Request requestParams) {
            var request = new Uri("https://api.openai.com/v1/chat/completions").SendPOST();
            return request.WithAuthorization(apiKey).WithJsonContent(requestParams).GetResult<VisionGpt.Response>();
        }

        /// <summary> See https://platform.openai.com/docs/guides/chat </summary>
        public Task<ChatGpt.Response> ChatGpt(ChatGpt.Request conversation) {
            var request = new Uri("https://api.openai.com/v1/chat/completions").SendPOST();
            return request.WithAuthorization(apiKey).WithJsonContent(conversation).GetResult<ChatGpt.Response>();
        }

        public class Text {

            public class CompletionsResponse {

                public string id { get; set; }
                public string @object { get; set; }
                public int created { get; set; }
                public string model { get; set; }
                public List<Choice> choices { get; set; }
                public Usage usage { get; set; }

                public class Usage {
                    public int prompt_tokens { get; set; }
                    public int completion_tokens { get; set; }
                    public int total_tokens { get; set; }
                }

                public class Choice {
                    public string text { get; set; }
                    public int index { get; set; }
                    public object logprobs { get; set; }
                    public string finish_reason { get; set; }
                }

            }

        }

        public class Image {

            /// <summary> See https://platform.openai.com/docs/api-reference/images/create </summary>
            public class Request {

                /// <summary> See https://platform.openai.com/docs/api-reference/images/create#images-create-prompt </summary>
                public string prompt { get; set; }

                /// <summary> See https://platform.openai.com/docs/api-reference/images/create#images-create-model </summary>
                public string model { get; set; } = "dall-e-3"; //"dall-e-2";

                /// <summary> See https://platform.openai.com/docs/api-reference/images/create#images-create-n </summary>
                public int n { get; set; } = 1;

                /// <summary> See https://platform.openai.com/docs/api-reference/images/create#images-create-size </summary>
                public string size { get; set; } = "1024x1024";

                /// <summary> See https://platform.openai.com/docs/api-reference/images/create#images-create-quality </summary>
                public string quality { get; set; } = "standard";

                /// <summary> See https://platform.openai.com/docs/api-reference/images/create#images-create-style </summary>
                public string style { get; set; } = "vivid";

            }

            public class Response {
                public int created { get; set; }
                public List<ImageEntry> data { get; set; }

                public class ImageEntry {
                    public string url { get; set; }
                }

            }

        }
        /// <summary> https://platform.openai.com/docs/guides/text-to-speech/text-to-speech
        /// </summary>
        public async Task<Stream> TextToSpeech(Audio.TTSRequest requestParam) {
            return await new Uri("https://api.openai.com/v1/audio/speech").SendPOST().WithAuthorization(apiKey)
                .WithJsonContent(requestParam).GetResult<Stream>().CopyToSeekableStreamIfNeeded(true);
        }

        /// <summary> https://platform.openai.com/docs/guides/speech-to-text
        /// </summary>
        public async Task<Audio.STTResponse> SpeechToText(Audio.STTRequest requestParam) {
            Dictionary<string, object> formContent = new Dictionary<string, object>();
            formContent.Add("model", requestParam.model);
            formContent.Add("language", requestParam.language);
            formContent.Add("responseFormat", requestParam.responseFormat);
            formContent.Add("temperature", requestParam.temperature);
            if (requestParam.prompt != null) {
                formContent.Add("prompt", requestParam.prompt);
            }

            RestRequest uri = new Uri("https://api.openai.com/v1/audio/transcriptions").SendPOST().WithAuthorization(apiKey)
                .AddStreamViaForm(requestParam.fileStream, "speech.mp3").WithFormContent(formContent);
            return await uri.GetResult<Audio.STTResponse>();
        }

        public class Audio {

            /// <summary>
            /// https://platform.openai.com/docs/api-reference/audio/createSpeech
            /// </summary>
            public class TTSRequest {
                
                /// <summary> https://platform.openai.com/docs/api-reference/audio/createSpeech#audio-createspeech-input </summary>
                public string input { get; set; }
                
                /// <summary> https://platform.openai.com/docs/api-reference/audio/createSpeech#audio-createspeech-model </summary>
                public string model { get; set; } = "tts-1";
                
                /// <summary> https://platform.openai.com/docs/api-reference/audio/createSpeech#audio-createspeech-voice
                /// (and https://platform.openai.com/docs/guides/text-to-speech/voice-options for samples) </summary>
                public string voice { get; set; } = "onyx";
                
                /// <summary> https://platform.openai.com/docs/api-reference/audio/createSpeech#audio-createspeech-response_format </summary>
                public string response_format { get; set; } = "mp3";
                
                /// <summary> https://platform.openai.com/docs/api-reference/audio/createSpeech#audio-createspeech-speed </summary>
                public double speed { get; set; } = 1.0;
                
            }

            /// <summary>
            /// https://platform.openai.com/docs/api-reference/audio/createTranscription
            /// </summary>
            public class STTRequest {
                public Stream fileStream { get; set; }
                public string model { get; set; } = "whisper-1";
                public string language { get; set; } = "en";
                public string prompt { get; set; }
                public string responseFormat { get; set; } = "text";
                public int temperature { get; set; } = 0;

            }

            public class STTResponse {
                public string text { get; set; }
            }
        }

    }
    public class ChatGpt {

        public class Message {

            public readonly string role;
            public readonly string content;

            public AudioResponse audio { get; set; }

            /// <summary> https://platform.openai.com/docs/guides/structured-outputs/refusals </summary>
            public readonly string refusal;

            [JsonConstructor]
            public Message(string role, string content, string refusal) {
                this.role = role;
                this.content = content;
                this.refusal = refusal;
            }

            public Message(Role role, string content) {
                this.role = role.ToString();
                this.content = content;
            }

            public class AudioResponse {
                public string data { get; set; }
                public byte[] dataAsBytes => Convert.FromBase64String(data);
            }

        }

        public enum Role { system, user, assistant }

        public class Request {

            /// <summary> See https://beta.openai.com/docs/models/overview </summary>
            public string model = "gpt-4o-mini";
            
            /// <summary> https://platform.openai.com/docs/api-reference/chat/create#chat-create-temperature </summary>
            public double? temperature { get; set; }

            /// <summary> The maximum number of tokens to generate in the completion.
            /// The token count of your prompt plus max_tokens cannot exceed the model's context length.
            /// Most models have a context length of 2048 tokens (except for the newest models, which support 4096). </summary>
            public int max_tokens { get; set; }
            public List<Message> messages { get; set; }

            /// <summary> e.g. "text", "audio" to be able to return both text and audio </summary>
            public List<string> modalities { get; set; }
            
            /// <summary> https://platform.openai.com/docs/api-reference/chat/create#chat-create-audio </summary>
            public AudioParameters audio { get; set; }

            /// <summary> typically null, but if the AI e.g. should respond only with json it should be ChatGpt.Request.ResponseFormat.json </summary>
            public ResponseFormat response_format { get; set; }

            public Request(List<Message> messages, int max_tokens = 4096) {
                var tokenCountForMessages = JsonWriter.GetWriter(this).Write(messages).Length;
                if (max_tokens + tokenCountForMessages > 4096) {
                    max_tokens = 4096 - tokenCountForMessages;
                }
                this.messages = messages;
                this.max_tokens = max_tokens;
            }

            /// <summary> https://platform.openai.com/docs/guides/audio/quickstart?audio-generation-quickstart-example=audio-out </summary>
            public class AudioParameters {
                
                /// <summary> Voices options see https://platform.openai.com/docs/guides/text-to-speech/voice-options </summary>
                public string voice { get; set; }
                
                /// <summary> Usable formats are wav, mp3, flac, opus, pcm16 (From https://platform.openai.com/docs/api-reference/chat/create#chat-create-audio ) </summary>
                public string format { get; set; } = "mp3";
                
            }

            public class ResponseFormat {

                /// <summary> See https://platform.openai.com/docs/guides/text-generation/json-mode </summary>
                [Obsolete("Use ResponseFormat.jsonSchema instead")]
                public static ResponseFormat json => new ResponseFormat() { type = "json_object" };

                /// <summary> See https://platform.openai.com/docs/guides/structured-outputs/how-to-use </summary>
                public static ResponseFormat NewJsonSchema(string name, JsonSchema schema) => new ResponseFormat() {
                    type = "json_schema",
                    json_schema = new JsonSchemaResponse(name, schema, strict: true)
                };

                public string type { get; set; }

                /// <summary> The json schema of the response, see
                /// https://platform.openai.com/docs/guides/structured-outputs/how-to-use?context=without_parse
                /// </summary>
                public JsonSchemaResponse json_schema { get; set; }

                public class JsonSchemaResponse {

                    public string name { get; set; }

                    public JsonSchema schema { get; set; }

                    /// <summary> https://platform.openai.com/docs/guides/structured-outputs/how-to-use?context=without_parse </summary>
                    public bool strict { get; set; }

                    public JsonSchemaResponse(string name, JsonSchema schema, bool strict) {
                        this.name = name;
                        this.schema = schema;
                        this.strict = strict;
                    }

                }

            }

        }

        public class Response {

            public string id { get; set; }
            public string @object { get; set; }
            public int created { get; set; }
            public string model { get; set; }
            public Usage usage { get; set; }
            public List<Choice> choices { get; set; }

            public class Choice {
                public Message message { get; set; }
                public string finish_reason { get; set; }
                public int index { get; set; }
            }

            public class Usage {
                public int prompt_tokens { get; set; }
                public int completion_tokens { get; set; }
                public int total_tokens { get; set; }
            }

        }

    }

    public class VisionGpt {

        /// <summary>
        /// Line class for representing a line of inquiry to VisionGPT. The field content needs to be 
        /// generic object to support both one line string and List of string:object dictionary as content field
        /// Reference: https://platform.openai.com/docs/guides/vision/quick-start
        /// </summary>
        public class Line {

            public readonly string role;
            public readonly object content;
            /// <summary> https://platform.openai.com/docs/guides/structured-outputs/refusals </summary>
            public readonly string refusal;

            public Line(string role, List<Dictionary<string, object>> content) {
                this.role = role;
                this.content = content;
            }

            public Line(ChatGpt.Role role, List<Dictionary<string, object>> content) {
                this.role = role.ToString();
                this.content = content;
            }

            [JsonConstructor]
            public Line(string role, string content, string refusal) {
                this.role = role;
                this.content = content;
                this.refusal = refusal;
            }

            public Line(ChatGpt.Role role, string content) {
                this.role = role.ToString();
                this.content = content;
            }
        }

        public class Request {

            /// <summary> See https://beta.openai.com/docs/models/overview </summary>
            public string model = "gpt-4o";

            /// <summary> The maximum number of tokens to generate in the completion.
            /// The token count of your prompt plus max_tokens cannot exceed the model's context length.
            /// Most models have a context length of 2048 tokens (except for the newest models, which support 4096). </summary>
            public int max_tokens { get; set; }
            public List<Line> messages { get; set; }

            public Request(List<Line> messages, int max_tokens = 4096) {
                var tokenCountForMessages = JsonWriter.GetWriter(this).Write(messages).Length;
                if (max_tokens + tokenCountForMessages > 4096) {
                    max_tokens = 4096 - tokenCountForMessages;
                }
                this.messages = messages;
                this.max_tokens = max_tokens;
            }
        }

        public class Response {

            public string id { get; set; }
            public string @object { get; set; }
            public int created { get; set; }
            public string model { get; set; }
            public Usage usage { get; set; }
            public List<Choice> choices { get; set; }

            public class Choice {
                public Line message { get; set; }
                public string finish_reason { get; set; }
                public int index { get; set; }
            }

            public class Usage {
                public int prompt_tokens { get; set; }
                public int completion_tokens { get; set; }
                public int total_tokens { get; set; }
            }

        }

    }

    public static class ChatGptExtensions {

        public static void AddUserLine(this ICollection<ChatGpt.Message> self, string userMessage) {
            self.Add(new ChatGpt.Message(ChatGpt.Role.user, content: userMessage));
        }

        public static void AddValidExampleSchemaResponses<T>(this ICollection<ChatGpt.Message> self, params T[] exampleResponses) {
            self.Add(new ChatGpt.Message(ChatGpt.Role.system, content: GetSchemaExamplesString(exampleResponses)));
        }

        [Obsolete("Use strict json schema mode instead of providing the schema as an input line")]
        public static void AddUserLineWithJsonResultStructure<T>(this ICollection<ChatGpt.Message> self, string userMessage, T exampleResponse) {
            self.Add(new ChatGpt.Message(ChatGpt.Role.user, content: userMessage));
            self.Add(new ChatGpt.Message(ChatGpt.Role.system, content: CreateJsonInstructions(exampleResponse)));
        }

        [Obsolete("Use strict json schema mode instead of providing the schema as an input line", true)]
        public static string CreateJsonInstructions<T>(params T[] exampleResponses) {
            if (exampleResponses.IsNullOrEmpty()) throw new InvalidOperationException();
            var exampleResponse = exampleResponses[0];
            var jsonSchema = JsonWriter.GetWriter(exampleResponse).Write(CreateJsonSchema(exampleResponse));
            var jsonSchemaInfos = " This is the json schema that describes the format you have to use for your json response: " + jsonSchema;
            return jsonSchemaInfos + GetSchemaExamplesString(exampleResponses);
        }

        private static string GetSchemaExamplesString<T>(T[] exampleResponses) {
            var exampleJsonInfos = " For the provided json schema, these would be examples of a valid response:";
            foreach (T exampleResponse in exampleResponses) {
                var exampleJson = JsonWriter.GetWriter(exampleResponses).Write(exampleResponses);
                exampleJsonInfos += " " + exampleJson;
            }
            return exampleJsonInfos;
        }

        public static JsonSchema CreateJsonSchema<T>(T exampleResponse) {
            var schemaGenerator = new ModelToJsonSchema(nullValueHandling: NullValueHandling.Ignore);
            var className = typeof(T).Name;
            var jsonSchema = schemaGenerator.ToJsonSchema(className, exampleResponse);
            jsonSchema.additionalProperties = false;
            return jsonSchema;
        }

        public static T ParseNewLineContentAsJson<T>(this ChatGpt.Message newLine) {
            var responseText = (string)newLine.content;
            if (responseText.StartsWith("```json\n")) {
                responseText = responseText.Replace("```json\n", "");
            }
            if (responseText.EndsWith("\n```")) {
                responseText = responseText.Replace("\n```", "");
            }
            return JsonReader.GetReader().Read<T>(responseText);
        }

    }

    public static class VisionGptExtention {

        public static void AddImageURL(this ICollection<VisionGpt.Line> self, string url) {
            var content = new List<Dictionary<string, object>>();

            content.Add(new Dictionary<string, object>() {
                { "type", "image_url" }, {
                    "image_url", new Dictionary<string, string> {
                        { "url", url },
                        { "detail", "high" }
                    }
                },
            });
            self.Add(new VisionGpt.Line(ChatGpt.Role.user, content: content));
        }
        public static void AddQuestionsToImage(this ICollection<VisionGpt.Line> self, string url, List<string> questions) {
            var content = new List<Dictionary<string, object>>();
            content.Add(new Dictionary<string, object>() {
                { "type", "text" },
                { "text", "Rate the following questions with a confidence from 0 to 100 based on how well the question fits the image" },
            });

            content.Add(new Dictionary<string, object>() {
                { "type", "image_url" }, {
                    "image_url", new Dictionary<string, string> {
                        { "url", url },
                        { "detail", "high" }
                    }
                },
            });
            questions.ForEach(question => {
                content.Add(new Dictionary<string, object>() {
                    { "type", "text" },
                    { "text", question },
                });
            });

            self.Add(new VisionGpt.Line(ChatGpt.Role.user, content: content));
        }

        [Obsolete("Use strict json schema mode instead of providing the schema as an input line")]
        public static void AddUserLineWithJsonResultStructure<T>(this ICollection<VisionGpt.Line> self, string userMessage, T exampleResponse) {
            self.Add(new VisionGpt.Line(ChatGpt.Role.user, content: userMessage));
            self.Add(new VisionGpt.Line(ChatGpt.Role.system, content: ChatGptExtensions.CreateJsonInstructions(exampleResponse)));
        }

        public static T ParseNewLineContentAsJson<T>(this VisionGpt.Line newLine) {
            var responseText = (string)newLine.content;
            if (responseText.StartsWith("```json\n")) {
                responseText = responseText.Replace("```json\n", "");
            }
            if (responseText.EndsWith("\n```")) {
                responseText = responseText.Replace("\n```", "");
            }
            try {
                return JsonReader.GetReader(null).Read<T>(responseText);
            } catch (Exception e) {
                Log.e($"Failed to parse the response as json: {responseText}", e);
                Debugger.Break();
                throw;
            }
        }

    }

    public static class ChatGptAudioExtensions {

        public static async Task<Stream> TextToSpeech(this OpenAi openAi, string textToSay, string usedVoice) {
            var responseTTS = await openAi.TextToSpeech(new OpenAi.Audio.TTSRequest() {
                input = textToSay,
                model = "tts-1",
                voice = usedVoice
            });
            return responseTTS;
        }

        public static async Task<byte[]> TextToSpeechWithFurtherInstructions(this OpenAi openai, string textToSay, string voiceActorInstructions, string usedVoice, double temperature = 1.2) {
            var messages = new List<ChatGpt.Message>() {
                new ChatGpt.Message(ChatGpt.Role.system, content: $"{voiceActorInstructions}"),
                new ChatGpt.Message(ChatGpt.Role.user, content: textToSay),
            };
            var request = new ChatGpt.Request(messages);
            request.model = "gpt-4o-audio-preview"; // https://platform.openai.com/docs/guides/audio/quickstart?audio-generation-quickstart-example=audio-out
            request.modalities = new List<string> { "text", "audio" };
            request.audio = new ChatGpt.Request.AudioParameters() { voice = usedVoice };
            request.temperature = temperature;
            var response = await openai.ChatGpt(request);
            ChatGpt.Message message = response.choices.Single().message;
            return message.audio.dataAsBytes;
        }

    }

}