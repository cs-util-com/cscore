using System.Text;
using Newtonsoft.Json;
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
        string input = "Hello World";

        using (HttpClient client = new HttpClient())
        {
            SetBearerToken(client, openAiKey);

            HttpResponseMessage response = await MakeTTSRequest(client, input);
            Assert.True(response.IsSuccessStatusCode);

            byte[] responseContent = await response.Content.ReadAsByteArrayAsync();
            Assert.True(responseContent.Length > 0);

            string outputPath = audioFolderPath + "speech.mp3";

            File.WriteAllBytes(outputPath, responseContent);
            Assert.True(File.Exists(outputPath));

            Console.WriteLine("Saved audio to " + outputPath);

        }
    }
    void SetBearerToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }
    async Task<HttpResponseMessage> MakeTTSRequest(HttpClient client, string input)
    {
        MyTTSRequest ttsRequest = new MyTTSRequest(input);
        string jsonPayload = JsonConvert.SerializeObject(ttsRequest);
        StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(openAISpeechURL, content);
        return response;
    }

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