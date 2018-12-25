using System.IO;

namespace com.csutil {

    public static class FileExtensions {

        public static DirectoryInfo GetChildDir(this DirectoryInfo self, string childFolder) {
            return new DirectoryInfo(self.FullPath() + Path.DirectorySeparatorChar + childFolder);
        }

        public static FileInfo GetChild(this DirectoryInfo self, string childName) {
            return new FileInfo(self.FullPath() + Path.DirectorySeparatorChar + childName);
        }

        public static DirectoryInfo CreateIfNeeded(this DirectoryInfo self) { if (!self.Exists) { self.Create(); } return self; }

        public static string FullPath(this FileSystemInfo self) { return Path.GetFullPath("" + self); }

        public static DirectoryInfo ParentDir(this FileInfo self) { return self.Directory; }

        public static FileInfo SetExtension(this FileInfo self, string newExtension) {
            return new FileInfo(Path.ChangeExtension(self.FullPath(), newExtension));
        }

    }

}