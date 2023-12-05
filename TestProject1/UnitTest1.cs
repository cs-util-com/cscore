using com.csutil;
using com.csutil.http;
using Xunit.Abstractions;
using Zio;
using Zio.FileSystems;
namespace SPPTest;

public class UnitTest1
{
    private readonly ITestOutputHelper output;


    static string openAIAudioURL = "https://api.openai.com/v1/audio/";
    static string currentDirectory = Directory.GetCurrentDirectory();
    static string audioFolderPath = Path.GetFullPath(Path.Combine(currentDirectory, @"../../../audio/"));
    static string openAiKey = File.ReadAllText(Path.GetFullPath(Path.Combine(currentDirectory, @"../../../env.txt")));
    public UnitTest1(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public async Task ExampleTTS()
    {
        HttpResponseMessage response = await TTS(new Audio.TTSRequest() { input = "hello world" });
        Assert.NotNull(response);

        string outputPath = audioFolderPath + "speech.mp3";
        File.WriteAllBytes(outputPath, await response.Content.ReadAsByteArrayAsync());
        Assert.NotNull(Path.GetFileName(outputPath));
        output.WriteLine(outputPath);
    }

    [Fact]
    public async Task ExampleSTT()
    {
        string outputPath = audioFolderPath + "speech.mp3";

        Audio.STTResponse response = await STT(new Audio.STTRequest() { file = outputPath });
        Assert.NotEmpty(response.text);
    }


    public Task<HttpResponseMessage> TTS(Audio.TTSRequest requestParam)
    {
        return new Uri(openAIAudioURL + "speech").SendPOST().WithAuthorization(openAiKey).WithJsonContent(requestParam).GetResult<HttpResponseMessage>();
    }

    public Task<Audio.STTResponse> STT(Audio.STTRequest requestParam)
    {
        Dictionary<string, object> formContent = new Dictionary<string, object>
        {
            { "model", requestParam.model },
        };
        string outputPath = audioFolderPath + "speech.mp3";

        IFileSystem fs = new MemoryFileSystem();
        UPath filePath1 = "/speech.mp3";
        fs.WriteAllBytes(filePath1, File.ReadAllBytes(outputPath));
        FileEntry fe = fs.GetFileEntry(filePath1);

        RestRequest uri = new Uri(openAIAudioURL + "transcriptions").SendPOST().WithAuthorization(openAiKey).AddFileViaForm(fe).WithFormContent(formContent);
        return uri.GetResult<Audio.STTResponse>();
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
            public string file { get; set; }
            public string model { get; set; } = "whisper-1";
            public string language { get; set; } = "en";
            public string prompt { get; set; }
            public string response_format { get; set; } = "text";
            public int temperature { get; set; } = 0;

        }

        public class STTResponse
        {
            public string text { get; set; }
        }
    }

}