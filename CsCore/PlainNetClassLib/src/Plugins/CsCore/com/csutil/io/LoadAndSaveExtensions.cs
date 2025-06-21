using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Zio;

namespace com.csutil {
    public static class LoadAndSaveExtensions {

        public static void SaveStream(this FileInfo self, Stream streamToSave) {
            using (var fileStream = File.Create(self.FullName)) { streamToSave.CopyTo(fileStream); }
        }

        public static FileStream LoadAsStream(this FileInfo self, FileMode fileMode = FileMode.Open,
                               FileAccess fileAccess = FileAccess.Read, FileShare fileShare = FileShare.Read) {
            return File.Open(self.FullPath(), fileMode, fileAccess, fileShare);
        }

        public static T LoadAs<T>(this FileInfo self, IJsonReader jsonReader, FileShare fileShare = FileShare.Read) {
            using (FileStream stream = self.LoadAsStream(fileShare: fileShare)) {
                return stream.LoadAs<T>(jsonReader);
            }
        }

        public static T LoadAs<T>(this FileEntry self, IJsonReader jsonReader, FileShare fileShare = FileShare.Read) {
            using (Stream stream = self.Open(FileMode.Open, FileAccess.Read, fileShare)) {
                return stream.LoadAs<T>(jsonReader);
            }
        }

        public static object LoadAs(this FileInfo self, Type type, IJsonReader jsonReader, FileShare fileShare = FileShare.Read) {
            using (FileStream stream = File.Open(self.FullPath(), FileMode.Open, FileAccess.Read, fileShare)) {
                return stream.LoadAs(type, jsonReader);
            }
        }

        public static object LoadAs(this FileEntry self, Type type, IJsonReader jsonReader, FileShare fileShare = FileShare.Read) {
            using (Stream stream = self.Open(FileMode.Open, FileAccess.Read, fileShare)) {
                return stream.LoadAs(type, jsonReader);
            }
        }

        [Obsolete("Use version where IJsonReader is passed as argument")]
        public static T LoadAs<T>(this Stream self) {
            return LoadAs<T>(self, JsonReader.GetReader());
        }

        public static T LoadAs<T>(this Stream self, IJsonReader jsonReader) {
            using (StreamReader s = new StreamReader(self)) {
                if (typeof(T) == typeof(string)) {
                    return (T)(object)s.ReadToEnd();
                }
                return jsonReader.Read<T>(s);
            }
        }

        [Obsolete("Use version where IJsonReader is passed as argument")]
        public static object LoadAs(this Stream self, Type t) {
            return LoadAs(self, t, JsonReader.GetReader());
        }

        public static object LoadAs(this Stream self, Type t, IJsonReader jsonReader) {
            using (StreamReader s = new StreamReader(self)) {
                if (t == typeof(string)) {
                    return s.ReadToEnd();
                }
                return jsonReader.ReadAsType(s, t);
            }
        }

        [Obsolete("Use version where IJsonReader is passed as argument")]
        public static void SaveAsJson<T>(this FileInfo self, T objectToSave) {
            SaveAsJson(self, objectToSave, JsonWriter.GetWriter(objectToSave));
        }

        public static void SaveAsJson<T>(this FileInfo self, T objectToSave, IJsonWriter jsonWriter) {
            using (StreamWriter streamWriter = File.CreateText(self.FullPath())) {
                streamWriter.SaveAsJson(objectToSave, jsonWriter);
            }
        }

        [Obsolete("Use version where IJsonWriter is passed as argument")]
        public static void SaveAsJson<T>(this FileEntry self, T objectToSave, bool asPrettyString = false) {
            SaveAsJson(self, objectToSave, JsonWriter.GetWriter(objectToSave), asPrettyString);
        }

        public static void SaveAsJson<T>(this FileEntry self, T objectToSave, IJsonWriter jsonWriter, bool asPrettyString = false) {
            if (asPrettyString) {
                SaveAsText(self, jsonWriter.AsPrettyString(objectToSave));
                return;
            }

            using (var stream = self.OpenOrCreateForWrite()) {
                stream.SetLength(0); // Reset the stream in case it was opened
                using (StreamWriter streamWriter = new StreamWriter(stream)) {
                    streamWriter.SaveAsJson(objectToSave, jsonWriter);
                }
            }
        }

        public static Stream OpenOrCreateForWrite(this FileEntry self, FileShare share = FileShare.None) {
            return self.Open(FileMode.OpenOrCreate, FileAccess.Write, share);
        }

        public static Stream OpenOrCreateForReadWrite(this FileEntry self, FileShare share = FileShare.Read) {
            return self.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, share);
        }

        public static Stream OpenForRead(this FileEntry self, FileMode fileMode = FileMode.Open,
                               FileAccess fileAccess = FileAccess.Read, FileShare fileShare = FileShare.Read) {
            return self.Open(fileMode, fileAccess, fileShare);
        }

        public static void SaveStream(this FileEntry self, Stream streamToSave, Action<long> onProgress = null, bool resetStreamToStart = true) {
            self.Parent.CreateV2();
            if (resetStreamToStart) {
                if (!streamToSave.CanSeek) { throw new InvalidOperationException("Stream not seekable, cant jump back to start of stream, use resetStreamToStart = false or first do stream.CopyToSeekableStreamIfNeeded()"); }
                streamToSave.ResetStreamCurserPositionToBeginning();
            }
            using (var fileStream = self.OpenOrCreateForWrite()) {
                fileStream.SetLength(0); // Reset the stream in case it was opened
                if (onProgress == null) {
                    streamToSave.CopyTo(fileStream);
                } else {
                    streamToSave.CopyTo(fileStream, onProgress);
                }
            }
            if (resetStreamToStart) { streamToSave.ResetStreamCurserPositionToBeginning(); }
        }

        public static async Task SaveStreamAsync(this FileEntry self, Stream streamToSave, Action<long> onProgress = null, bool resetStreamToStart = true) {
            self.Parent.CreateV2();
            if (resetStreamToStart) {
                if (!streamToSave.CanSeek) { throw new InvalidOperationException("Stream not seekable, cant jump back to start of stream, use resetStreamToStart = false or first do stream.CopyToSeekableStreamIfNeeded()"); }
                streamToSave.ResetStreamCurserPositionToBeginning();
            }
            using (var fileStream = self.OpenOrCreateForWrite()) {
                fileStream.SetLength(0); // Reset the stream in case it was opened
                if (onProgress == null) {
                    await streamToSave.CopyToAsync(fileStream);
                } else {
                    await streamToSave.CopyToAsync(fileStream, onProgress);
                }
            }
            if (resetStreamToStart) { streamToSave.ResetStreamCurserPositionToBeginning(); }
        }

        [Obsolete("Use version where IJsonWriter is passed as argument")]
        public static void SaveAsJson<T>(this StreamWriter self, T objectToSave) {
            SaveAsJson(self, objectToSave, JsonWriter.GetWriter(objectToSave));
        }

        public static void SaveAsJson<T>(this StreamWriter self, T objectToSave, IJsonWriter jsonWriter) {
            jsonWriter.Write(objectToSave, self);
        }

        [Obsolete("Use version where IJsonReader is passed as argument")]
        /// <summary> This method helps with decrypting the string before parsing it as a json object </summary>
        public static T LoadAsEncyptedJson<T>(this FileInfo self, string jsonEncrKey, Func<T> getDefaultValue) {
            return LoadAsEncyptedJson(self, jsonEncrKey, getDefaultValue, JsonReader.GetReader());
        }

        /// <summary> This method helps with decrypting the string before parsing it as a json object </summary>
        public static T LoadAsEncyptedJson<T>(this FileInfo self, string jsonEncrKey, Func<T> getDefaultValue, IJsonReader jsonReader) {
            try {
                return jsonReader.Read<T>(self.LoadAs<string>(null).Decrypt(jsonEncrKey));
            } catch (Exception e) {
                Log.w("" + e);
                return getDefaultValue();
            }
        }

        [Obsolete("Use version where IJsonWriter is passed as argument")]
        public static void SaveAsEncryptedJson<T>(this FileInfo self, T objectToSave, string jsonEncrKey) {
            SaveAsEncryptedJson(self, objectToSave, jsonEncrKey, JsonWriter.GetWriter(objectToSave));
        }

        public static void SaveAsEncryptedJson<T>(this FileInfo self, T objectToSave, string jsonEncrKey, IJsonWriter jsonWriter) {
            self.SaveAsText(jsonWriter.Write(objectToSave).Encrypt(jsonEncrKey));
        }

        public static void SaveAsText(this FileInfo self, string text) {
            self.ParentDir().Create();
            File.WriteAllText(self.FullPath(), text, Encoding.UTF8);
        }

        public static void SaveAsText(this FileEntry self, string text) {
            self.Parent.CreateV2();
            using (var s = self.OpenOrCreateForWrite()) { s.WriteAsText(text); }
        }

        public static void WriteAsText(this Stream self, string text) {
            self.SetLength(0); // Reset the stream in case it was opened
            using (var w = new StreamWriter(self, Encoding.UTF8)) { w.Write(text); }
        }

    }

}