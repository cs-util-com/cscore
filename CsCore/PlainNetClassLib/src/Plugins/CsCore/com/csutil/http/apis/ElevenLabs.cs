using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace com.csutil.http.apis {

    /// <summary> ElevenLabs client for Text-to-Speech operations. </summary>
    public class ElevenLabs {

        public const string Callum = "N2lVS1w4EtoT3dr4eOWO";

        private const string baseUrl = "https://api.elevenlabs.io";
        private readonly string apiKey;

        public ElevenLabs(string apiKey) { this.apiKey = apiKey; }

        /// <summary> Converts text to speech using a specified voice ID. </summary>
        /// <param name="text">The text to be converted into speech.</param>
        /// <param name="voiceId">The ID of the voice to be used. Defaults to a specific voice ID if not provided.</param>
        /// <returns>A task that represents the asynchronous operation, containing the audio stream of the synthesized speech.</returns>
        public Task<Stream> TextToSpeech(string text, string voiceId = Callum) {
            return TextToSpeech(voiceId, new TTSRequest { text = text });
        }

        /// <summary> Converts text to speech using a specified voice ID and additional parameters. </summary>
        /// <param name="voiceId">The ID of the voice to be used.</param>
        /// <param name="requestParams">The parameters for the text-to-speech request.</param>
        /// <param name="enableLogging">Optional. When set to false, full privacy mode is enabled, disabling history features. Default is null.</param>
        /// <param name="optimizeStreamingLatency">Optional. Deprecated. Controls latency optimizations at some cost of quality. Default is null.</param>
        /// <param name="outputFormat">Optional. Specifies the output audio format. Default is null.
        /// Can be e.g. one of these: 'mp3_22050_32', 'mp3_44100_32', 'mp3_44100_64', 'mp3_44100_96', 'mp3_44100_128', 'mp3_44100_192', 'pcm_16000', 'pcm_22050', 'pcm_24000', 'pcm_44100', 'ulaw_8000' </param>
        /// <returns>A task that represents the asynchronous operation, containing the audio stream of the synthesized speech.</returns>
        public async Task<Stream> TextToSpeech(string voiceId, TTSRequest requestParams, bool? enableLogging = null, int? optimizeStreamingLatency = null, string outputFormat = null) {
            // Construct the URL for the text-to-speech API endpoint.
            var url = new Uri($"{baseUrl}/v1/text-to-speech/{voiceId}");

            // Initialize a dictionary to hold query parameters.
            var queryParams = new Dictionary<string, object>();
            if (enableLogging.HasValue) {
                queryParams.Add("enable_logging", enableLogging.Value.ToString().ToLowerInvariant());
            }
            if (optimizeStreamingLatency.HasValue) {
                queryParams.Add("optimize_streaming_latency", optimizeStreamingLatency.Value);
            }
            if (!string.IsNullOrEmpty(outputFormat)) {
                queryParams.Add("output_format", outputFormat);
            }

            // Prepare the HTTP POST request with the necessary headers and content.
            var request = url.WithAddedQueryParams(queryParams).SendPOST();
            request = request.WithRequestHeader("xi-api-key", apiKey);
            request = request.WithJsonContent(requestParams);

            // Execute the request and handle any errors.
            await request.ThrowIfErrorStatus();

            // Retrieve the audio stream result from the response.
            var audioResult = await request.GetResult<Stream>();

            // Ensure the stream is seekable if necessary and return it.
            return await audioResult.CopyToSeekableStreamIfNeeded(true);
        }

        /// <summary> Represents the parameters for a text-to-speech request. </summary>
        public class TTSRequest {
            /// <summary> Required. The text to be converted into speech. </summary>
            public string text;

            /// <summary> Optional. Identifier of the model to be used. Defaults to "eleven_monolingual_v1". </summary>
            public string model_id = "eleven_monolingual_v1";

            /// <summary> Optional. Language code (ISO 639-1) to enforce a language for the model. Currently, only certain models support this. </summary>
            public string language_code;

            /// <summary> Optional. Voice settings overriding stored settings for the given voice, applied only to the current request. </summary>
            public VoiceSettings voice_settings;

            /// <summary> Optional. A list of pronunciation dictionary locators to be applied to the text, in order. Up to 3 locators per request. </summary>
            public List<PronunciationDictionaryLocator> pronunciation_dictionary_locators;

            /// <summary> Optional. If specified, the system will attempt deterministic sampling, so repeated requests with the same seed and parameters should return the same result. Must be an integer between 0 and 4294967295. </summary>
            public uint? seed;

            /// <summary> Optional. The text that came before the current request's text. Can improve prosody flow when concatenating multiple generations or influence the current generation's prosody. </summary>
            public string previous_text;

            /// <summary> Optional. The text that comes after the current request's text. Can improve prosody flow when concatenating multiple generations or influence the current generation's prosody. </summary>
            public string next_text;

            /// <summary> Optional. A list of request IDs of samples generated before this generation. Can improve prosody flow when splitting a large task into multiple requests. Up to 3 request IDs can be sent. </summary>
            public List<string> previous_request_ids;

            /// <summary> Optional. A list of request IDs of samples to be generated after this generation. Can improve prosody flow when splitting a large task into multiple requests. Up to 3 request IDs can be sent. </summary>
            public List<string> next_request_ids;

            /// <summary> Optional. Controls text normalization with three modes: 'auto' (default), 'on', and 'off'. When set to 'auto', the system decides whether to apply text normalization. With 'on', text normalization is always applied; with 'off', it is skipped. Cannot be turned on for certain models. </summary>
            public string apply_text_normalization = "auto";
        }

        /// <summary> Represents optional voice settings that can override stored settings for a single text-to-speech request. </summary>
        public class VoiceSettings {
            /// <summary> Optional. Adjusts the stability of the voice output. </summary>
            public float? stability;

            /// <summary> Optional. Adjusts the similarity boost for the voice output. </summary>
            public float? similarity_boost;
        }

        /// <summary> Represents a locator for a custom pronunciation dictionary, identified by ID and version. </summary>
        public class PronunciationDictionaryLocator {
            /// <summary> The ID of the pronunciation dictionary. </summary>
            public string id;

            /// <summary> The version ID of the pronunciation dictionary. </summary>
            public string version_id;
        }

    }

}