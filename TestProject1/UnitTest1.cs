using com.csutil;
namespace SPPTest;

public class UnitTest1
{

    static string openAISpeechURL = "https://api.openai.com/v1/audio/speech";

    static string currentDirectory = Directory.GetCurrentDirectory();
    static string audioFolderPath = Path.GetFullPath(Path.Combine(currentDirectory, @"../../../audio/"));
    static string openAiKey = File.ReadAllText(Path.GetFullPath(Path.Combine(currentDirectory, @"../../../env.txt")));


    [Fact]
    public async Task ExampleTTS()
    {
        HttpResponseMessage response = await TTS(new Audio.TTSRequest() { input = "hello world" });
        Assert.NotNull(response);

        string outputPath = audioFolderPath + "speech.mp3";
        File.WriteAllBytes(outputPath, await response.Content.ReadAsByteArrayAsync());
        Assert.NotNull(Path.GetFileName(outputPath));
    }

    public async Task ExampleSTT()
    {
        //TODO
    }


    public Task<HttpResponseMessage> TTS(Audio.TTSRequest requestParam)
    {
        return new Uri(openAISpeechURL).SendPOST().WithAuthorization(openAiKey).WithJsonContent(requestParam).GetResult<HttpResponseMessage>();
    }

    public Task<HttpResponseMessage> STT(Audio.STTRequest requestparam)
    {
        //TODO
        return null;
    }
    public class Audio
    {
        public class TTSRequest
        {
            public string input { get; set; }
            public string model { get; set; } = "tts-1";
            public string voice { get; set; } = "alloy";
            public string response_format { get; set; } = "mp3";
            public double speed { get; set; } = 1.0;
        }

        public class STTRequest
        {
            //TODO
        }

        public class STTResponse
        {
            public string text { get; set; }
        }
    }

}