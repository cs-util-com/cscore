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

        /// <summary>
        /// Gets the remaining path after the <see cref="prefix"/>.
        /// </summary>
        /// <param name="prefix">The prefix of the path.</param>
        /// <param name="self">The path to search.</param>
        /// <returns>The path after the prefix, or a <c>null</c> path if <see cref="path"/> does not have the correct prefix.</returns>
        public static UPath RemovePrefix(this UPath self, UPath prefix) {
            if (prefix.IsEmpty) { throw new InvalidDataException("The passed prefix cant be emtpy, must minimum be the UPath.root"); }
            if (!self.IsInDirectory(prefix, true)) { throw new InvalidDataException($"Path '{self}' is not in '{prefix}'"); }
            var remaining = self.FullName.Substring(prefix.FullName.Length);
            return new UPath(remaining).ToAbsolute();
        }

        public static UPath AddPrefix(this UPath self, UPath prefix) {
            return prefix.IsNull ? self : prefix / self.ToRelative();
        }

        /// <summary>
        /// Gets the first directory of the specified path and return the remaining path (/a/b/c, first directory: /a, remaining: b/c)
        /// </summary>
        /// <param name="path">The path to extract the first directory and remaining.</param>
        /// <param name="remainingPath">The remaining relative path after the first directory</param>
        /// <returns>The first directory of the path.</returns>
        /// <exception cref="ArgumentNullException">if path is <see cref="UPath.IsNull"/></exception>
        public static UPath GetFirstPart(this UPath path) {
            path.AssertNotNull();
            var fullname = path.FullName;
            var index = fullname.IndexOf(UPath.DirectorySeparator, 1);
            if (index < 0) { return fullname; }
            return fullname.Substring(0, index);
        }

    }
}