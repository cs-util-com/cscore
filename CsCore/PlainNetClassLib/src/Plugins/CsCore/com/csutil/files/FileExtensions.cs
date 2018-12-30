using System;
using System.IO;
using System.Text;
using System.Threading;

namespace com.csutil {

    public static class FileExtensions {

        public static DirectoryInfo GetChildDir(this DirectoryInfo self, string childFolder, bool assertThatChildMustExist = false) {
            var c = new DirectoryInfo(self.FullPath() + Path.DirectorySeparatorChar + childFolder);
            if (assertThatChildMustExist) { AssertV2.IsTrue(c.IsNotNullAndExists(), "childFolder '" + childFolder + "' does not exist! Full path: " + c.FullPath()); }
            return c;
        }

        public static FileInfo GetChild(this DirectoryInfo self, string childFile, bool assertThatChildMustExist = false) {
            var c = new FileInfo(self.FullPath() + Path.DirectorySeparatorChar + childFile);
            if (assertThatChildMustExist) { AssertV2.IsTrue(c.IsNotNullAndExists(), "childFile '" + childFile + "' does not exist! Full path: " + c.FullPath()); }
            return c;
        }

        public static DirectoryInfo CreateV2(this DirectoryInfo self) {
            if (!self.Exists) { self.Create(); self.Refresh(); }
            return self;
        }

        public static string FullPath(this FileSystemInfo self) {
            return Path.GetFullPath("" + self);
        }

        public static bool IsNotNullAndExists(this FileSystemInfo self) {
            if (self == null) { return false; } else { return self.Exists; }
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
            return DeleteV2(self, () => { self.Delete(deleteAlsoIfNotEmpty); });
        }

        private static bool DeleteV2(FileSystemInfo self, Action deleteAction) {
            if (self != null && self.Exists) {
                deleteAction();
                self.Refresh();
                AssertV2.IsFalse(self.Exists, "Still exists: " + self.FullPath());
                return true;
            }
            return false;
        }

        public static void MoveToV2(this DirectoryInfo self, DirectoryInfo target) {
            // if (target.Exists) { throw Log.e("Cant move dir to already existing " + target.FullPath()); }
            self.MoveTo(target.FullPath());
            self.Refresh();
            target.Refresh();
        }

        // From https://stackoverflow.com/a/3822913/165106
        public static void CopyTo(this DirectoryInfo self, DirectoryInfo target) {
            var sourcePath = self.FullPath();
            var targetPath = target.FullPath();
            // Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }
            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories)) {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
            target.Refresh();
        }

        public static T LoadAs<T>(this FileInfo self) {
            var s = File.ReadAllText(self.FullPath(), Encoding.UTF8);
            if (typeof(T) == typeof(string)) { return (T)(object)s; }
            return JsonReader.GetReader().Read<T>(s);
        }

        public static void SaveAsJson<T>(this FileInfo self, T objectToSave) {
            if (typeof(T) == typeof(string)) {
                self.SaveText((string)(object)objectToSave);
            } else {
                self.SaveText(JsonWriter.GetWriter().Write(objectToSave));
            }
        }

        private static void SaveText(this FileInfo self, string text) {
            File.WriteAllText(self.FullPath(), text, Encoding.UTF8);
        }
    }

}