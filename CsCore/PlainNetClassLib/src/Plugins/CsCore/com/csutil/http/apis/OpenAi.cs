using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using com.csutil.model.jsonschema;
using Newtonsoft.Json;

namespace com.csutil.http.apis
{

    public class OpenAi
    {

        private string apiKey;

        /// <summary> </summary>
        /// <param name="apiKey"> See https://beta.openai.com/docs/api-reference/authentication and https://beta.openai.com/account/api-keys </param>
        public OpenAi(string apiKey) { this.apiKey = apiKey; }

        /// <summary> See also https://platform.openai.com/docs/guides/chat/chat-vs-completions : "Because gpt-3.5-turbo performs at a
        /// similar capability to text-davinci-003 but at 10% the price per token, we recommend gpt-3.5-turbo for most use cases." </summary>
        [Obsolete("This API is deprecated, use .ChatGpt(..) instead")]
        public Task<Text.CompletionsResponse> Complete(string prompt)
        {
            return Complete(new Text.CompletionsRequest() { prompt = prompt });
        }

        [Obsolete("This API is deprecated, use .ChatGpt(..) instead")]
        public Task<Text.CompletionsResponse> Complete(Text.CompletionsRequest requestParams)
        {
            var request = new Uri("https://api.openai.com/v1/completions").SendPOST();
            return request.WithAuthorization(apiKey).WithJsonContent(requestParams).GetResult<Text.CompletionsResponse>();
        }

        /// <summary> https://beta.openai.com/docs/api-reference/images/create </summary>
        public Task<Image.Response> TextToImage(Image.Request requestParams)
        {
            var request = new Uri("https://api.openai.com/v1/images/generations").SendPOST();
            return request.WithAuthorization(apiKey).WithJsonContent(requestParams).GetResult<Image.Response>();
        }

        /// <summary> https://beta.openai.com/docs/api-reference/images/create </summary>
        public Task<ChatGpt.Response> ImageToText(Vision.Request requestParams, string imageURL)
        {
            var request = new Uri("https://api.openai.com/v1/chat/completions").SendPOST();

            var content = new List<Dictionary<string, object>>();
            content.Add(new Dictionary<string, object>()
            {
                {"type","text"},
                {"text","What's in this image?"},
            });

            content.Add(new Dictionary<string, object>()
            {
                {"type","image_url"},
                {"image_url",new Dictionary<string,string>{
                    {"url",imageURL},
                    {"detail","high"}
                }},
            });

            var message = new Vision.Request.Message()
            {
                role = "user",
                content = content
            };
            var messages = new List<Vision.Request.Message>()
            {
                message

            };

            var finalRequestParams = new Vision.Request()
            {
                messages = messages
            };


            return request.WithAuthorization(apiKey).WithJsonContent(finalRequestParams).GetResult<ChatGpt.Response>();
        }


        public class Vision
        {
            public class Request
            {
                public string model { get; set; } = "gpt-4-vision-preview";
                public int max_tokens { get; set; } = 300;
                public List<Message> messages { get; set; }

                public class Message
                {
                    public string role { get; set; } = "user";

                    public List<Dictionary<string, object>> content { get; set; }
                }
            }
        }

        /// <summary> See https://platform.openai.com/docs/guides/chat </summary>
        public Task<ChatGpt.Response> ChatGpt(ChatGpt.Request conversation)
        {
            var request = new Uri("https://api.openai.com/v1/chat/completions").SendPOST();
            return request.WithAuthorization(apiKey).WithJsonContent(conversation).GetResult<ChatGpt.Response>();
        }

        public class Text
        {

            /// <summary> See https://platform.openai.com/docs/api-reference/completions </summary>
            [Obsolete("This API is deprecated, use .ChatGpt(..) instead")]
            public class CompletionsRequest
            {

                /// <summary> The prompt(s) to generate completions for, encoded as a string, array of strings, array of tokens, or array of token arrays.
                /// Note that <|endoftext|> is the document separator that the model sees during training, so if a prompt is not specified the
                /// model will generate as if from the beginning of a new document. </summary>
                public string prompt { get; set; }

                /// <summary> See https://beta.openai.com/docs/models/overview </summary>
                public string model { get; set; } = "text-davinci-003";

                /// <summary> What sampling temperature to use. Higher values means the model will take more risks.
                /// Try 0.9 for more creative applications, and 0 (argmax sampling) for ones with a well-defined answer.
                /// We generally recommend altering this or top_p but not both. </summary>
                public double temperature { get; set; } = 0.7;

                /// <summary> The maximum number of tokens to generate in the completion.
                /// The token count of your prompt plus max_tokens cannot exceed the model's context length.
                /// Most models have a context length of 2048 tokens (except for the newest models, which support 4096). </summary>
                public int max_tokens { get; set; } = 256;

                /// <summary> An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the
                /// tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered.
                /// We generally recommend altering this or temperature but not both. </summary>
                public int top_p { get; set; } = 1;

                /// <summary> Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency in the text so far,
                /// decreasing the model's likelihood to repeat the same line verbatim </summary>
                public int frequency_penalty { get; set; } = 0;

                /// <summary> Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the text so far,
                /// increasing the model's likelihood to talk about new topics </summary>
                public int presence_penalty { get; set; } = 0;

                /// <summary> The suffix that comes after a completion of inserted text </summary>
                public string suffix { get; set; }

                /// <summary> How many completions to generate for each prompt.
                /// Note: Because this parameter generates many completions, it can quickly consume your token quota.
                /// Use carefully and ensure that you have reasonable settings for max_tokens and stop </summary>
                public int n { get; set; } = 1;

                /// <summary> Echo back the prompt in addition to the completion </summary>
                public bool echo { get; set; } = false;

                /// <summary> Generates best_of completions server-side and returns the "best" (the one with the highest log probability per token).
                /// Results cannot be streamed. When used with n, best_of controls the number of candidate completions and n specifies how many to
                /// return – best_of must be greater than n. Note: Because this parameter generates many completions,
                /// it can quickly consume your token quota. Use carefully and ensure that you have reasonable settings for max_tokens and stop. </summary>
                public int best_of { get; set; } = 1;

                /// <summary> Modify the likelihood of specified tokens appearing in the completion.
                /// Accepts a json object that maps tokens (specified by their token ID in the GPT tokenizer) to an associated bias value from -100 to 100.
                /// You can use this tokenizer tool (which works for both GPT-2 and GPT-3) to convert text to token IDs. Mathematically, the bias is added to
                /// the logits generated by the model prior to sampling. The exact effect will vary per model, but values between -1 and 1 should decrease or
                /// increase likelihood of selection; values like -100 or 100 should result in a ban or exclusive selection of the relevant token.
                /// As an example, you can pass {"50256": -100} to prevent the <|endoftext|> token from being generated. </summary>
                public Dictionary<string, object> logit_bias { get; set; } = new Dictionary<string, object>();

                /// <summary> A unique identifier representing your end-user, which will help OpenAI to monitor and detect abuse. </summary>
                public string user { get; set; } = "";

            }

            public class CompletionsResponse
            {

                public string id { get; set; }
                public string @object { get; set; }
                public int created { get; set; }
                public string model { get; set; }
                public List<Choice> choices { get; set; }
                public Usage usage { get; set; }

                public class Usage
                {
                    public int prompt_tokens { get; set; }
                    public int completion_tokens { get; set; }
                    public int total_tokens { get; set; }
                }

                public class Choice
                {
                    public string text { get; set; }
                    public int index { get; set; }
                    public object logprobs { get; set; }
                    public string finish_reason { get; set; }
                }

            }

        }

        public class Image
        {

            /// <summary> See https://platform.openai.com/docs/api-reference/images/create </summary>
            public class Request
            {

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

            public class Response
            {
                public int created { get; set; }
                public List<ImageEntry> data { get; set; }

                public class ImageEntry
                {
                    public string url { get; set; }
                }

            }

        }

    }

    public class ChatGpt
    {

        public class Line
        {

            public readonly string role;
            public readonly string content;

            [JsonConstructor]
            public Line(string role, string content)
            {
                this.role = role;
                this.content = content;
            }

            public Line(Role role, string content)
            {
                this.role = role.ToString();
                this.content = content;
            }

        }

        public enum Role { system, user, assistant }

        public class Request
        {

            /// <summary> See https://beta.openai.com/docs/models/overview </summary>
            public string model = "gpt-3.5-turbo-1106";

            /// <summary> The maximum number of tokens to generate in the completion.
            /// The token count of your prompt plus max_tokens cannot exceed the model's context length.
            /// Most models have a context length of 2048 tokens (except for the newest models, which support 4096). </summary>
            public int max_tokens { get; set; }
            public List<Line> messages { get; set; }

            /// <summary> typically null, but if the AI e.g. should respond only with json it should be ChatGpt.Request.ResponseFormat.json </summary>
            public ResponseFormat response_format { get; set; }

            public Request(List<Line> messages, int max_tokens = 4096)
            {
                var tokenCountForMessages = JsonWriter.GetWriter(this).Write(messages).Length;
                if (max_tokens + tokenCountForMessages > 4096)
                {
                    max_tokens = 4096 - tokenCountForMessages;
                }
                this.messages = messages;
                this.max_tokens = max_tokens;
            }

            public class ResponseFormat
            {
                /// <summary> See https://platform.openai.com/docs/guides/text-generation/json-mode </summary>
                public static ResponseFormat json = new ResponseFormat() { type = "json_object" };
                public string type { get; set; }
            }

        }

        public class Response
        {

            public string id { get; set; }
            public string @object { get; set; }
            public int created { get; set; }
            public string model { get; set; }
            public Usage usage { get; set; }
            public List<Choice> choices { get; set; }

            public class Choice
            {
                public Line message { get; set; }
                public string finish_reason { get; set; }
                public string finish_details { get; set; }
                public int index { get; set; }
            }

            public class Usage
            {
                public int prompt_tokens { get; set; }
                public int completion_tokens { get; set; }
                public int total_tokens { get; set; }
            }

        }

    }

    public static class ChatGptExtensions
    {

        public static void AddUserLineWithJsonResultStructure<T>(this ICollection<ChatGpt.Line> self, string userMessage, T exampleResponse)
        {
            self.Add(new ChatGpt.Line(ChatGpt.Role.user, content: userMessage));
            self.Add(new ChatGpt.Line(ChatGpt.Role.system, content: CreateJsonInstructions(exampleResponse)));
        }

        public static string CreateJsonInstructions<T>(T exampleResponse)
        {
            var schemaGenerator = new ModelToJsonSchema(nullValueHandling: Newtonsoft.Json.NullValueHandling.Ignore);
            var className = typeof(T).Name;
            JsonSchema schema = schemaGenerator.ToJsonSchema(className, exampleResponse);
            var schemaJson = JsonWriter.GetWriter(exampleResponse).Write(schema);
            var exampleJson = JsonWriter.GetWriter(exampleResponse).Write(exampleResponse);
            var jsonSchemaInfos = " This is the json schema that describes the format you have to use for your json response: " + schemaJson;
            var exampleJsonInfos = " And for that schema, this would an example of a valid response: " + exampleJson;
            return jsonSchemaInfos + exampleJsonInfos;
        }

        public static T ParseNewLineContentAsJson<T>(this ChatGpt.Line newLine)
        {
            var responseText = newLine.content;
            if (responseText.StartsWith("```json\n"))
            {
                responseText = responseText.Replace("```json\n", "");
            }
            if (responseText.EndsWith("\n```"))
            {
                responseText = responseText.Replace("\n```", "");
            }
            return JsonReader.GetReader().Read<T>(responseText);
        }

    }

    [Obsolete("Not needed anymore, use the OpenAi class instead")]
    public class OpenAiLabs
    {

        private string apiKey;

        /// <summary> </summary>
        /// <param name="apiKey"> See https://beta.openai.com/docs/api-reference/authentication and https://beta.openai.com/account/api-keys </param>
        public OpenAiLabs(string apiKey) { this.apiKey = apiKey; }

        public async Task<LabsApi.Response> SendLabsApiRequest(LabsApi.ApiTask apiTask)
        {
            var latestProgress = await SendLabsApiTask(apiTask);
            while (latestProgress.status == LabsApi.Response.STATUS_PENDING)
            {
                await TaskV2.Delay(2000); // Check status every 2 seconds
                // The API currently often randomly fails with server errors, add an exponential backoff layer to ignore these:
                latestProgress = await TaskV2.TryWithExponentialBackoff(() => GetLatestTaskProgress(latestProgress.id), maxNrOfRetries: 5, initialExponent: 10);
            }
            return latestProgress;
        }

        private Task<LabsApi.Response> SendLabsApiTask(LabsApi.ApiTask taskToCreate)
        {
            var createTaskUri = new Uri("https://labs.openai.com/api/labs/tasks").SendPOST();
            return createTaskUri.WithAuthorization(apiKey).WithJsonContent(taskToCreate).GetResult<LabsApi.Response>();
        }

        private Task<LabsApi.Response> GetLatestTaskProgress(string taskId)
        {
            var taskProgressUri = new Uri("https://labs.openai.com/api/labs/tasks/" + taskId).SendGET();
            return taskProgressUri.WithAuthorization(apiKey).GetResult<LabsApi.Response>();
        }

        public class LabsApi
        {

            public class ApiTask
            {

                /// <summary> Tasks available are "text2im" or "inpainting" </summary>
                public string task_type { get; set; }
                public Prompt prompt { get; set; }

                public class Prompt
                {
                    public string caption { get; set; }
                    public int batch_size { get; set; } = 2;
                }

                // TODO add inpainting Prompt subclass with additional image and
                // masked_image fields, see eg https://github.com/charlesjlee/twitter_dalle2_bot/blob/main/dalle2.py#L115

            }

            public class Response
            {

                public const string STATUS_SUCCESS = "succeeded";
                public const string STATUS_PENDING = "pending";
                public const string STATUS_FAILED = "failed";

                public string @object { get; set; }
                public string id { get; set; }
                public int created { get; set; }
                public string task_type { get; set; }
                /// <summary> Is "pending" until "succeeded" (then also the generations will be filled) </summary>
                public string status { get; set; }
                public StatusInformation status_information { get; set; }
                public string prompt_id { get; set; }
                public Prompt prompt { get; set; }

                public Generations generations { get; set; }

                public class Prompt
                {
                    public string id { get; set; }
                    public string @object { get; set; }
                    public int created { get; set; }
                    public string prompt_type { get; set; }
                    public Prompt prompt { get; set; }
                    public object parent_generation_id { get; set; }
                    public string caption { get; set; }
                }

                public class StatusInformation
                {
                }

                public class Generation
                {
                    public string id { get; set; }
                    public string @object { get; set; }
                    public int created { get; set; }
                    public string generation_type { get; set; }
                    public GenerationData generation { get; set; }
                    public string task_id { get; set; }
                    public string prompt_id { get; set; }
                    public bool is_public { get; set; }
                }

                public class GenerationData
                {
                    public string image_path { get; set; }
                }

                public class Generations
                {
                    public string @object { get; set; }
                    public List<Generation> data { get; set; }
                }

            }

        }

    }

}