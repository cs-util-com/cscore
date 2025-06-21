// #define RUN_EXPENSIVE_TESTS

using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis;
using Xunit;

namespace com.csutil.integrationTests.http {

    public class ElevenLabsTests {

        public ElevenLabsTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        /// <summary>
        /// Verifies the basic functionality of the TextToSpeech method using the default voice.
        /// Ensures that a valid audio stream is returned for a given input text.
        /// </summary>
        #if RUN_EXPENSIVE_TESTS
        [Fact]
        #endif
        public async Task ExampleUsage1_TextToSpeech_DefaultVoice() {
            // Retrieve the ElevenLabs API key.
            var apiKey = await IoC.inject.GetAppSecrets().GetSecret("ElevenLabsKey");
            var elevenLabs = new ElevenLabs(apiKey);

            // Define the text to convert to speech.
            var textToSpeak = "Hello world! This is an ElevenLabs TTS test.";

            // Call the basic TextToSpeech method with default parameters.
            var audioResult = await elevenLabs.TextToSpeech(textToSpeak);

            // Validate that the audio stream is not null and contains data.
            Assert.NotNull(audioResult);
            using var audioData = await audioResult.CopyToSeekableStreamIfNeeded(true);
            Assert.True(audioData.Length > 0, "Expected some audio data from the TTS call.");

            // Log success and optionally save the audio file locally.
            Log.d("Successfully got TTS audio for text: " + textToSpeak);
            var targetFolder = EnvironmentV2.instance.GetOrAddTempFolder("ElevenLabsTests");
            var fileEntry = targetFolder.GetChild($"{DateTimeV2.Now.ToReadableStringExact()}_ExampleUsage1.mp3");
            await fileEntry.SaveStreamAsync(audioData);
            Log.d("Audio file saved to: " + fileEntry.GetFullFileSystemPath());
        }

        /// <summary>
        /// Tests the TextToSpeech method with a custom voice ID and additional parameters.
        /// Verifies that optional features like logging, latency optimization, and output format are applied correctly.
        /// </summary>
        #if RUN_EXPENSIVE_TESTS
        [Fact]
        #endif
        public async Task ExampleUsage2_TextToSpeech_AdvancedParameters() {
            var apiKey = await IoC.inject.GetAppSecrets().GetSecret("ElevenLabsKey");
            var elevenLabs = new ElevenLabs(apiKey);

            // Specify a custom voice ID and text for speech synthesis.
            var voices = await elevenLabs.GetVoices();
            var voiceId = voices.Single(x => x.name == "Callum").voice_id;
            var textToSpeak = "Hello there! Testing ElevenLabs with advanced parameters and the voice id received via the API.";

            // Build a TTS request with optional query parameters.
            var requestParams = new ElevenLabs.TTSRequest { text = textToSpeak };

            bool enableLogging = true;
            int optimizeLatency = 3;
            string outputFormat = "mp3_22050_32";

            // Call the TextToSpeech method with the specified parameters.
            var audioResult = await elevenLabs.TextToSpeech(
                voiceId, requestParams,
                enableLogging: enableLogging,
                optimizeStreamingLatency: optimizeLatency,
                outputFormat: outputFormat
            );

            // Verify the returned audio stream is valid.
            Assert.NotNull(audioResult);
            using var audioData = await audioResult.CopyToSeekableStreamIfNeeded(true);
            Assert.True(audioData.Length > 0, "Expected audio data to be returned.");

            // Log the result and save the audio locally for verification.
            Log.d($"Got advanced TTS audio from voice '{voiceId}' with {audioData.Length} bytes.");
            var targetFolder = EnvironmentV2.instance.GetOrAddTempFolder("ElevenLabsTests");
            var fileEntry = targetFolder.GetChild($"{DateTimeV2.Now.ToReadableStringExact()}_ExampleUsage2_{voiceId}.mp3");
            await fileEntry.SaveStreamAsync(audioData);
            Log.d("Audio file saved to: " + fileEntry.GetFullFileSystemPath());
        }

        /// <summary>
        /// Demonstrates the use of voice settings overrides, such as stability and similarity boost.
        /// Verifies that the overridden settings produce valid audio output.
        /// </summary>
        #if RUN_EXPENSIVE_TESTS
        [Fact]
        #endif
        public async Task ExampleUsage3_TextToSpeech_WithVoiceSettings() {
            var apiKey = await IoC.inject.GetAppSecrets().GetSecret("ElevenLabsKey");
            var elevenLabs = new ElevenLabs(apiKey);

            var voiceId = ElevenLabs.Callum;
            var textToSpeak = "Testing voice settings like stability and similarity boost on ElevenLabs.";

            var ttsRequest = new ElevenLabs.TTSRequest {
                text = textToSpeak,
                voice_settings = new ElevenLabs.VoiceSettings {
                    stability = 0.5f,
                    similarity_boost = 0.8f
                }
            };

            // Call TextToSpeech with custom voice settings.
            var audioResult = await elevenLabs.TextToSpeech(voiceId, ttsRequest);
            Assert.NotNull(audioResult);
            using var audioData = await audioResult.CopyToSeekableStreamIfNeeded(true);
            Assert.True(audioData.Length > 0);

            // Log and save the audio file.
            Log.d($"Got TTS audio with voice settings. Data length: {audioData.Length} bytes.");
            var targetFolder = EnvironmentV2.instance.GetOrAddTempFolder("ElevenLabsTests");
            var fileEntry = targetFolder.GetChild($"{DateTimeV2.Now.ToReadableStringExact()}_ExampleUsage3_{voiceId}_voiceSettings.mp3");
            await fileEntry.SaveStreamAsync(audioData);
            Log.d("Audio file saved to: " + fileEntry.GetFullFileSystemPath());
        }

    }

}