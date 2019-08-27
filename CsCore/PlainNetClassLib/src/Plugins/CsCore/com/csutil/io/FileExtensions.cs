using com.csutil.encryption;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace com.csutil {

    public static class FileExtensions {

        public static DirectoryInfo GetChildDir(this DirectoryInfo self, string childFolder, bool assertThatChildMustExist = false) {
            var c = new DirectoryInfo(self.FullPath() + childFolder);
            if (assertThatChildMustExist) {
                AssertV2.IsTrue(c.IsNotNullAndExists(), "childFolder '" + childFolder + "' doesnt exist! Path=" + c.FullPath());
            }
            return c;
        }

        public static FileInfo GetChild(this DirectoryInfo self, string childFile, bool assertThatChildMustExist = false) {
            var c = new FileInfo(self.FullPath() + childFile);
            if (assertThatChildMustExist) {
                AssertV2.IsTrue(c.IsNotNullAndExists(), "childFile '" + childFile + "' doesnt exist! Path=" + c.FullPath());
            }
            return c;
        }

        public static DirectoryInfo CreateV2(this DirectoryInfo self) {
            if (!self.ExistsV2()) { self.Create(); self.Refresh(); }
            return self;
        }

        /// <summary> 
        /// After renaming a Directory its .Name property does not return the correct folder name anymore! 
        /// Thats why NameV2 has to be used instead, which is more expensive but always updated correctly
        /// </summary>
        public static string NameV2(this DirectoryInfo self) {
            var p = self.FullName;
            if (p.EndsWith(Path.DirectorySeparatorChar)) {
                p = p.Substring("" + Path.DirectorySeparatorChar, false);
            }
            return Path.GetFileName(p); // GetFileName works for folders if the path doesnt end in /
        }

        public static string FullPath(this DirectoryInfo self) {
            var p = self.FullName;
            if (!p.EndsWith(Path.DirectorySeparatorChar)) { return p + Path.DirectorySeparatorChar; }
            return p;
        }

        public static string FullPath(this FileInfo self) { return self.FullName; }

        public static bool IsNotNullAndExists(this FileSystemInfo self) {
            if (self == null) { return false; } else { return self.ExistsV2(); }
        }

        public static bool ExistsV2(this FileSystemInfo self) {
            self.Refresh();
            return self.Exists;
        }

        public static DirectoryInfo ParentDir(this FileInfo self) {
            return self.Directory;
        }

        public static FileInfo SetExtension(this FileInfo self, string newExtension) {
            return new FileInfo(Path.ChangeExtension(self.FullPath(), newExtension));
        }

        public static bool DeleteV2(this FileSystemInfo self) {
            return DeleteV2(self, () => { self.Delete(); });
        }

        public static bool DeleteV2(this DirectoryInfo self, bool deleteAlsoIfNotEmpty = true) {
            return DeleteV2(self, () => {
                if (deleteAlsoIfNotEmpty) { // Recursively delete all children first:
                    foreach (var subDir in self.GetDirectories()) { subDir.DeleteV2(deleteAlsoIfNotEmpty); }
                    foreach (var file in self.GetFiles()) { file.DeleteV2(); }
                    self.Refresh();
                    AssertV2.IsTrue(self.IsEmtpy(), "Dir was not empty after recursive cleanup!");
                }
                self.Delete(deleteAlsoIfNotEmpty);
            });
        }

        public static bool IsEmtpy(this DirectoryInfo self) {
            return !Directory.EnumerateFileSystemEntries(self.FullName).Any();
        }

        private static bool DeleteV2(FileSystemInfo self, Action deleteAction) {
            if (self != null && self.ExistsV2()) {
                deleteAction();
                self.Refresh();
                AssertV2.IsFalse(self.ExistsV2(), "Still exists: " + self.FullName);
                return true;
            }
            return false;
        }

        public static bool Rename(this FileInfo self, string newName) {
            self.MoveTo(self.ParentDir().GetChild(newName).FullPath());
            return self.ExistsV2();
        }

        public static bool MoveToV2(this FileInfo self, DirectoryInfo target) {
            self.MoveTo(target.FullPath() + self.Name);
            target.Refresh();
            return self.ExistsV2();
        }

        public static bool OpenInExternalApp(this FileSystemInfo self) {
            try {
                System.Diagnostics.Process.Start(@self.FullName);
                return true;
            }
            catch (System.Exception e) { Log.e(e); }
            return false;
        }

        public static bool MoveToV2(this DirectoryInfo self, DirectoryInfo target) {
            self.MoveTo(target.FullPath());
            if (!target.ExistsV2()) {
                Log.w("Normal MoveTo did not work, will copy and delete instead");
                self.MoveViaCopyAndDelete(target);
            }
            return self.ExistsV2();
        }

        public static bool MoveViaCopyAndDelete(this DirectoryInfo self, DirectoryInfo target) {
            if (self.CopyTo(target)) {
                self.DeleteV2();
                return !self.ExistsV2();
            }
            return false;
        }

        public static bool Rename(this DirectoryInfo self, string newName) {
            var target = self.Parent.GetChildDir(newName);
            AssertV2.IsFalse(target.IsNotNullAndExists(), "Already exists: target=" + target.FullPath());
            return self.MoveToV2(target);
        }

        public static bool CopyTo(this DirectoryInfo self, DirectoryInfo target, bool replaceExisting = false) {
            if (!replaceExisting && target.IsNotNullAndExists()) {
                throw new ArgumentException("Cant copy to existing folder " + target);
            }
            var sourcePath = self.FullPath();
            var targetPath = target.FullPath();
            // From https://stackoverflow.com/a/3822913/165106
            // Create all empty directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }
            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories)) {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), overwrite: true);
            }
            return target.ExistsV2();
        }

        public static T LoadAs<T>(this FileInfo self) { return (T)LoadAs(self, typeof(T)); }

        public static object LoadAs(this FileInfo self, Type t) {
            using (FileStream readStream = File.Open(self.FullPath(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                using (StreamReader s = new StreamReader(readStream)) {
                    if (t == typeof(string)) { return s.ReadToEnd(); }
                    { // If a subscriber reacts to LoadAs return its response:
                        var results = EventBus.instance.NewPublishIEnumerable("LoadAs" + t, self);
                        var result = results.Filter(x => t.IsInstanceOfType(x)).FirstOrDefault();
                        if (result != null) { return result; }
                    } // Otherwise use the default json reader approach:
                    return JsonReader.GetReader().ReadAsType(s, t);
                }
            }
        }

        public static void SaveAsJson<T>(this FileInfo self, T objectToSave) {
            using (StreamWriter file = File.CreateText(self.FullPath())) {
                JsonWriter.GetWriter().Write(objectToSave, file);
            }
        }

        /// <summary> This method helps with decrypting the string before parsing it as a json object </summary>
        public static T LoadAsEncyptedJson<T>(this FileInfo self, string jsonEncrKey, Func<T> getDefaultValue) {
            try { return JsonReader.GetReader().Read<T>(self.LoadAs<string>().Decrypt(jsonEncrKey)); } catch (Exception e) { Log.w("" + e); return getDefaultValue(); }
        }

        public static void SaveAsEncryptedJson<T>(this FileInfo self, T objectToSave, string jsonEncrKey) {
            self.SaveAsText(JsonWriter.GetWriter().Write(objectToSave).Encrypt(jsonEncrKey));
        }

        public static void SaveAsText(this FileInfo self, string text) {
            self.ParentDir().CreateV2();
            File.WriteAllText(self.FullPath(), text, Encoding.UTF8);
        }

        public static long GetFileSize(this FileInfo self) { return self.Length; }

        public static string GetFileSizeString(this FileInfo self) {
            return ByteSizeToString.ByteSizeToReadableString(self.GetFileSize());
        }

    }

}