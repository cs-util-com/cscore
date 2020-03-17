using com.csutil.encryption;
using System;
using System.IO;
using System.Text;
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

        public static T LoadAs<T>(this FileInfo self, FileShare fileShare = FileShare.Read) {
            using (FileStream s = self.LoadAsStream(fileShare: fileShare)) { return s.LoadAs<T>(); }
        }

        public static T LoadAs<T>(this FileEntry self, FileShare fileShare = FileShare.Read) {
            using (var selff = self.Open(FileMode.Open, FileAccess.Read, fileShare)) { return selff.LoadAs<T>(); }
        }

        public static object LoadAs(this FileInfo self, Type t, FileShare fileShare = FileShare.Read) {
            using (FileStream s = File.Open(self.FullPath(), FileMode.Open, FileAccess.Read, fileShare)) { return s.LoadAs(t); }
        }

        public static object LoadAs(this FileEntry self, Type type, FileShare fileShare = FileShare.Read) {
            using (var selff = self.Open(FileMode.Open, FileAccess.Read, fileShare)) { return selff.LoadAs(type); }
        }

        public static T LoadAs<T>(this Stream self) {
            using (StreamReader s = new StreamReader(self)) {
                if (typeof(T) == typeof(string)) { return (T)(object)s.ReadToEnd(); }
                return JsonReader.GetReader().Read<T>(s);
            }
        }

        public static object LoadAs(this Stream self, Type t) {
            using (StreamReader s = new StreamReader(self)) {
                if (t == typeof(string)) { return s.ReadToEnd(); }
                return JsonReader.GetReader().ReadAsType(s, t);
            }
        }

        public static void SaveAsJson<T>(this FileInfo self, T objectToSave) {
            using (StreamWriter streamWriter = File.CreateText(self.FullPath())) { streamWriter.SaveAsJson(objectToSave); }
        }

        public static void SaveAsJson<T>(this FileEntry self, T objectToSave) {
            using (var stream = self.OpenOrCreate()) {
                using (StreamWriter streamWriter = new StreamWriter(stream)) { streamWriter.SaveAsJson(objectToSave); }
            }
        }

        public static Stream OpenOrCreate(this FileEntry self) { return self.Open(FileMode.OpenOrCreate, FileAccess.Write); }

        public static void SaveAsJson<T>(this StreamWriter self, T objectToSave) {
            JsonWriter.GetWriter().Write(objectToSave, self);
        }

        /// <summary> This method helps with decrypting the string before parsing it as a json object </summary>
        public static T LoadAsEncyptedJson<T>(this FileInfo self, string jsonEncrKey, Func<T> getDefaultValue) {
            try {
                return JsonReader.GetReader().Read<T>(self.LoadAs<string>().Decrypt(jsonEncrKey));
            } catch (Exception e) { Log.w("" + e); return getDefaultValue(); }
        }

        public static void SaveAsEncryptedJson<T>(this FileInfo self, T objectToSave, string jsonEncrKey) {
            self.SaveAsText(JsonWriter.GetWriter().Write(objectToSave).Encrypt(jsonEncrKey));
        }

        public static void SaveAsText(this FileInfo self, string text) {
            self.ParentDir().Create();
            File.WriteAllText(self.FullPath(), text, Encoding.UTF8);
        }

        public static void SaveAsText(this FileEntry self, string text) {
            using (var s = self.OpenOrCreate()) { s.WriteAsText(text); }
        }

        public static void WriteAsText(this Stream self, string text) { using (var w = new StreamWriter(self, Encoding.UTF8)) { w.Write(text); } }

    }

}