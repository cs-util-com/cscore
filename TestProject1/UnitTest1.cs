using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;
using com.csutil;
using com.csutil.http.apis;
using System.Diagnostics;

namespace SPPTest;

public class UnitTest1
{
    [Fact]
    public async Task ExampleUsage3_ChatGpt()
    {
        var key = "";
        var openAi = new OpenAi(key);
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
}