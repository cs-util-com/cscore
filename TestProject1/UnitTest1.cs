using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;
using com.csutil;
using com.csutil.http.apis;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Net;
using System.Text;
namespace SPPTest;

public class UnitTest1
{
    string openAiKey = File.ReadAllText(@"C:\Users\nicol\Desktop\VSCSem3\cscore\TestProject1\env.txt");

    // [Fact]
    // public async Task ExampleUsage3_ChatGpt()
    // {


    //     var openAi = new OpenAi(openAiKey);
    //     var messages = new List<ChatGpt.Line>() {
    //             new ChatGpt.Line(ChatGpt.Role.system, content: "You are a standup comedian. You are on stage and about to tell a joke."),
    //             new ChatGpt.Line(ChatGpt.Role.user, content: "Do you know the joke about the chicken that crossed the road?"),
    //             new ChatGpt.Line(ChatGpt.Role.assistant, content: "Yes I actually happen to know the best one of all chicken jokes."),
    //             new ChatGpt.Line(ChatGpt.Role.user, content: "Why did the chicken cross the road?"),
    //         };
    //     var response = await openAi.ChatGpt(new ChatGpt.Request(messages));
    //     ChatGpt.Line newLine = response.choices.Single().message;
    //     Assert.Equal("" + ChatGpt.Role.assistant, newLine.role);
    //     Assert.NotEmpty(newLine.content);

    //     messages.Add(newLine);
    //     Log.d("response.content=" + JsonWriter.AsPrettyString(messages));
    // }

    [Fact]
    public async Task ExampleTTS()
    {

        string apiUrl = "https://api.openai.com/v1/audio/speech";
        string token = openAiKey;

        // Create HttpClient
        using (HttpClient client = new HttpClient())
        {
            // Set the authorization header with the Bearer token
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Define the JSON payload
            string jsonPayload = "{\"model\": \"tts-1\", \"voice\": \"alloy\",\"input\": \"please work\"}";

            // Create the content for the request
            StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Send the POST request
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            // Check the response status
            if (response.IsSuccessStatusCode)
            {
                // Read and display the response content
                byte[] responseContent = await response.Content.ReadAsByteArrayAsync();
                string outputPath = @"C:\Users\nicol\Desktop\VSCSem3\cscore\TestProject1\response.mp3";
                File.WriteAllBytes(outputPath, responseContent);
                Console.WriteLine($"Success! Response: {responseContent}");

            }
            else
            {
                Console.WriteLine($"Error! Status Code: {response.StatusCode}");
            }
        }





        // call speech with await
        //var response = await Speech("Hello World");
    }
    public Task<HttpWebResponse> Speech(string input)
    {
        return CreateSpeechRequest(new MyTTSRequest(input));
    }
    public Task<HttpWebResponse> CreateSpeechRequest(MyTTSRequest requestObject)
    {
        var request = new Uri("https://api.openai.com/v1/audio/speech").SendPOST();
        var aa = request.WithAuthorization(openAiKey).WithJsonContent(requestObject);
        return aa.GetResult<HttpWebResponse>();
    }
    // create a class with fields model, input and voice
    public class MyTTSRequest
    {
        public string model { get; set; } = "tts-1";
        public string input { get; set; }
        public string voice { get; set; } = "alloy";
        public MyTTSRequest(string input)
        {
            this.input = input;
        }
    }

}