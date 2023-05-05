using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Zio;

namespace com.csutil {

    public static class AudioHelper {

        public static bool TryGetAudioTypeForFileExtension(this FileEntry self, out AudioType audioType) {
            return TryGetAudioTypeFor(self.ExtensionWithoutDot(), out audioType);
        }

        public static bool TryGetAudioTypeFor(string fileExtension, out AudioType audioType) {
            switch (fileExtension) {
                case "mp3":
                    audioType = AudioType.MPEG;
                    return true;
                case "mpeg":
                    audioType = AudioType.MPEG;
                    return true;
                case "mpg": // mp3= MPEG-1 Layer 3
                    audioType = AudioType.MPEG;
                    return true;
                case "ogg":
                    audioType = AudioType.OGGVORBIS;
                    return true;
                case "wav":
                    audioType = AudioType.WAV;
                    return true;
                case "aif":
                    audioType = AudioType.AIFF;
                    return true;
                case "aiff":
                    audioType = AudioType.AIFF;
                    return true;
                case "mod":
                    audioType = AudioType.MOD;
                    return true;
                case "it":
                    audioType = AudioType.IT;
                    return true;
                case "s3m":
                    audioType = AudioType.S3M;
                    return true;
                case "xm":
                    audioType = AudioType.XM;
                    return true;
                case "xma":
                    audioType = AudioType.XMA;
                    return true;
                default:
                    audioType = AudioType.UNKNOWN;
                    return false;
            }
        }

        public static Task<AudioClip> LoadAudioClip(this FileEntry self) {
            if (self.TryGetAudioTypeForFileExtension(out var audioType)) {
                return UnityWebRequestMultimedia.GetAudioClip(self.GetFileUri(), audioType).SendV2().GetResult<AudioClip>();
            }
            throw new InvalidDataException("The file is not a supported audio file: " + self);
        }

    }

}