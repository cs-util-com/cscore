using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.http.apis {

    public class OpenAi {

        private string apiKey;

        /// <summary> </summary>
        /// <param name="apiKey"> See https://beta.openai.com/docs/api-reference/authentication and https://beta.openai.com/account/api-keys </param>
        public OpenAi(string apiKey) { this.apiKey = apiKey; }

        public async Task<CompletionsResponse> Complete(string promt) {
            return await Complete(new CompletionsRequest() { prompt = promt });
        }

        public async Task<CompletionsResponse> Complete(CompletionsRequest requestParams) {
            var request = new Uri("https://api.openai.com/v1/completions").SendPOST();
            return await request.WithAuthorization(apiKey).WithJsonContent(requestParams).GetResult<CompletionsResponse>();
        }

        /// <summary> See https://beta.openai.com/docs/api-reference/completions </summary>
        public class CompletionsRequest {

            /// <summary> The prompt(s) to generate completions for, encoded as a string, array of strings, array of tokens, or array of token arrays.
            /// Note that <|endoftext|> is the document separator that the model sees during training, so if a prompt is not specified the
            /// model will generate as if from the beginning of a new document. </summary>
            public string prompt { get; set; }

            /// <summary> See https://beta.openai.com/docs/models/overview </summary>
            public string model { get; set; } = "text-davinci-002";

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
            public Dictionary<string, object> logit_bias { get; set; }

            /// <summary> A unique identifier representing your end-user, which will help OpenAI to monitor and detect abuse. </summary>
            public string user { get; set; }
            
        }

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

}