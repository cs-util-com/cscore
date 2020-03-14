using System.IO;
using Zio;
using Zio.FileSystems;

namespace com.csutil {
    public static class ZioExtensions {

        public static DirectoryEntry GetChildDir(this DirectoryEntry self, string subDirName) {
            return new DirectoryEntry(self.FileSystem, self.Path / subDirName);
        }

        public static FileEntry GetChild(this DirectoryEntry self, string fileName) {
            return new FileEntry(self.FileSystem, self.Path / fileName);
        }

        public static DirectoryEntry ToRootDirectoryEntry(this DirectoryInfo localDir) {
            var pfs = new PhysicalFileSystem();
            var fs = new SubFileSystem(pfs, pfs.ConvertPathFromInternal(localDir.FullName));
            return fs.GetDirectoryEntry(UPath.Root);
        }

    }
}