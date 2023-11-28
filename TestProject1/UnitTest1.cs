using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;
using com.csutil;
using com.csutil.http.apis;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
namespace SPPTest;

public class UnitTest1
{
    string openAiKey = File.ReadAllText("/Users/devicedev/Documents/RWTH/5 Semester/Holobuilder Lab/cscore/TestProject1/env.txt");

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
        // call speech with await
        var response = await Speech("Hello World");
    }
    public Task<OpenAi.Text.CompletionsResponse> Speech(string input)
    {
        return CreateSpeechRequest(new MyTTSRequest(input));
    }
    public Task<OpenAi.Text.CompletionsResponse> CreateSpeechRequest(MyTTSRequest requestObject)
    {
        var request = new Uri("https://api.openai.com/v1/audio/speech").SendPOST();
        return request.WithAuthorization(openAiKey).WithJsonContent(requestObject).GetResult<OpenAi.Text.CompletionsResponse>();
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