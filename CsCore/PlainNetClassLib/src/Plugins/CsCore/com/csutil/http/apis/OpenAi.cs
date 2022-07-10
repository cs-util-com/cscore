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

        public class CompletionsRequest {

            public string prompt { get; set; }
            public string model { get; set; } = "text-davinci-002";
            public double temperature { get; set; } = 0.7;
            public int max_tokens { get; set; } = 256;
            public int top_p { get; set; } = 1;
            public int frequency_penalty { get; set; } = 0;
            public int presence_penalty { get; set; } = 0;

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