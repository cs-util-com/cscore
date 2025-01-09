using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Zio;
using Zio.FileSystems;

namespace com.csutil {

    public static class ZioExtensions {

        public static DirectoryEntry GetChildDir(this DirectoryEntry self, string subDirName, bool sanitize = true) {
            subDirName.ThrowErrorIfNullOrEmpty("subDirName");
            if (sanitize) { subDirName = Sanitize.SanitizeToDirName(subDirName); }
            AssertV3.AreEqual(subDirName, Sanitize.SanitizeToDirName(subDirName));
            return ResolveDirectoryPath(self, subDirName);
        }

        public static FileEntry GetChild(this DirectoryEntry self, string fileName, bool sanitize = true) {
            fileName.ThrowErrorIfNullOrEmpty("fileName");
            if (sanitize) { fileName = Sanitize.SanitizeToFileName(fileName); }
            AssertV3.AreEqual(fileName, Sanitize.SanitizeToFileName(fileName));
            return ResolveFilePath(self, fileName);
        }

        public static void ClearAllEntries(this DirectoryEntry self) {
            foreach (var child in self.EnumerateEntries()) {
                child.DeleteV2();
            }
        }
        
        public static DirectoryEntry ResolveDirectoryPath(this DirectoryEntry self, string relativePath) {
            return new DirectoryEntry(self.FileSystem, self.Path / relativePath);
        }

        public static FileEntry ResolveFilePath(this DirectoryEntry self, string relativePath) {
            return new FileEntry(self.FileSystem, self.Path / relativePath);
        }

        /// <summary> Returns a root dir for an existing directory entry </summary>
        public static DirectoryEntry ToRootDirectoryEntry(this DirectoryInfo localDir) {
            if (!localDir.Exists) { // The SubFileSystem cant handle non existing roots
                throw new DirectoryNotFoundException("ToRootDirectoryEntry on non-existing dir: " + localDir);
            }
            return NewSubFileSystemFor(localDir.FullName).GetDirectoryEntry(UPath.Root);
        }

        private static SubFileSystem NewSubFileSystemFor(string fullPath) {
            FileSystem fileSystem = new PhysicalFileSystem();
            return new SubFileSystem(fileSystem, fileSystem.ConvertPathFromInternal(fullPath));
        }

        /// <summary> Takes the parent dir and uses this as a root dir, only works if parent dir exists! </summary>
        public static FileEntry ToFileEntryInNewRoot(this FileInfo self) {
            return self.ParentDir().ToRootDirectoryEntry().GetChild(self.Name);
        }

        private static string ExtractDiscPrefix(string absPath) {
            if (!HasDiscPrefix(absPath)) { throw new InvalidDataException("Path does not contain a disc prefix: " + absPath); }
            return absPath.Substring(0, 2);
        }

        private static bool HasDiscPrefix(string absPath) { return absPath[1] == ':'; }

        public static DirectoryEntry AsNewRootDir(this DirectoryEntry self) {
            return new SubFileSystem(self.FileSystem, self.Path).GetDirectoryEntry(UPath.Root);
        }

        private static object threadLock = new object();

        public static DirectoryEntry CreateV2(this DirectoryEntry self) {
            lock (threadLock) {
                if (!self.Exists) { self.Create(); }
            }
            return self;
        }

        public static bool Rename(this FileEntry self, string newName, out FileEntry result) {
            result = self.Parent.GetChild(newName);
            self.MoveTo(result.FullName);
            return result.Exists;
        }

        public static FileEntry CopyToV2(this FileEntry self, FileEntry target, bool replaceExisting) {
            if (self.FileSystem != target.FileSystem) {
                using (var t = target.OpenOrCreateForWrite()) {
                    t.SetLength(0); // Reset the stream in case it was opened
                    using (var s = self.OpenForRead()) { s.CopyTo(t); }
                }
                AssertV3.IsTrue(target.Exists, () => "Target did not exist after copy to was done: " + target);
                return target;
            }
            return self.CopyTo(target.Path, replaceExisting);
        }

        public static bool MoveToV2(this FileEntry self, DirectoryEntry target, out FileEntry result) {
            result = target.GetChild(self.Name);
            self.MoveTo(result.FullName);
            return result.Exists;
        }

        public static bool OpenInExternalApp(this FileSystemEntry self) {
            self.ThrowErrorIfNull("FileSystemEntry");
            try {
                System.Diagnostics.Process.Start(GetFullFileSystemPath(self));
                return true;
            }
            catch (Exception e) { Log.w("" + e); }
            return false;
        }

        public static bool Rename(this DirectoryEntry self, string newName, out DirectoryEntry result) {
            var target = self.Parent.GetChildDir(newName);
            if (target.Exists) { throw new IOException("Cant rename, already exists: " + target); }
            return self.MoveToV2(target, out result);
        }

        public static bool MoveToV2(this DirectoryEntry source, DirectoryEntry target, out DirectoryEntry result) {
            AssertNotIdentical(source, target);
            AssertV3.IsTrue(source.FileSystem == target.FileSystem, () => "Moving between different file systems not implemented");
            source.MoveTo(target.Path);
            if (!target.Exists) { throw new DirectoryNotFoundException("Could not move dir to " + target); }
            result = target;
            return target.Exists;
        }

        public static bool CopyTo(this DirectoryEntry source, DirectoryEntry target, bool replaceExisting = false) {
            AssertNotIdentical(source, target);
            if (!replaceExisting && target.IsNotNullAndExists()) {
                throw new ArgumentException("Cant copy to existing folder " + target);
            }
            foreach (var subDir in source.EnumerateDirectories()) {
                CopyTo(subDir, target.GetChildDir(subDir.Name), replaceExisting);
            }
            target.CreateV2();
            foreach (var file in source.EnumerateFiles()) {
                var to = target.GetChild(file.Name);
                var createdFile = file.CopyToV2(to, replaceExisting);
                AssertV3.IsTrue(createdFile.Exists, () => "!createdFile.Exists: " + createdFile);
            }
            return target.Exists;
        }

        private static void AssertNotIdentical(DirectoryEntry source, DirectoryEntry target) {
            if (Equals(source.Path, target.Path)) {
                throw new OperationCanceledException("Identical source & target: " + source);
            }
        }

        public static bool IsNotNullAndExists(this FileSystemEntry self) {
            if (self == null) { return false; } else { return self.Exists; }
        }

        /// <summary>
        /// Gets the remaining path after the <see cref="prefix"/>.
        /// </summary>
        /// <param name="self">The path to search.</param>
        /// <param name="prefix">The prefix of the path.</param>
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
            string pathString = path.FullName;
            var index = pathString.IndexOf(UPath.DirectorySeparator, 1);
            if (index < 0) { return pathString; }
            return pathString.Substring(0, index);
        }

        public static bool DeleteV2(this FileSystemEntry self) {
            return DeleteV2(self, () => {
                self.Delete();
                return true;
            });
        }

        public static bool DeleteV2(this DirectoryEntry self, bool deleteAlsoIfNotEmpty = true) {
            return DeleteV2(self, () => {
                if (deleteAlsoIfNotEmpty) { // Recursively delete all children first:
                    if (!self.IsEmtpy()) {
                        foreach (var subDir in self.GetDirectories()) { subDir.DeleteV2(deleteAlsoIfNotEmpty); }
                        foreach (var file in self.GetFiles()) { file.DeleteV2(); }
                    }
                }
                if (!self.IsEmtpy()) { throw new IOException("Cant delete non-emtpy dir: " + self); }
                try {
                    self.Delete();
                    return true;
                } catch (Exception e) { Log.e(e); }
                return false;
            });
        }

        public static IEnumerable<DirectoryEntry> GetDirectories(this DirectoryEntry self) { return self.EnumerateDirectories(); }

        public static IEnumerable<FileEntry> GetFiles(this DirectoryEntry self) { return self.EnumerateFiles(); }

        private static bool DeleteV2(FileSystemEntry self, Func<bool> deleteAction) {
            if (self.IsNotNullAndExists()) {
                var res = deleteAction();
                AssertV3.IsFalse(!res || self.Exists, () => "Still exists: " + self);
                return res;
            }
            return false;
        }

        public static bool IsEmtpy(this DirectoryEntry self) { // TODO use old method to avoid exceptions?
            try { return !self.EnumerateEntries().Any(); } catch (Exception) { return true; }
        }

        /// <summary> Returns file size in bytes, use with <see cref="ByteSizeToString"/> </summary>
        public static long GetFileSize(this FileEntry self) { return self.Length; }

        public static string GetFileSizeString(this FileEntry self) {
            return ByteSizeToString.ByteSizeToReadableString(self.GetFileSize());
        }

        [Obsolete("Use ExtractZipIntoDir instead")]
        public static void ExtractIntoDir(this FileEntry self, DirectoryEntry targetDir) { ExtractZipIntoDir(self, targetDir); }

        /// <summary> Currently only works when working with a physical file system for the target directory </summary>
        public static void ExtractZipIntoDir(this FileEntry self, DirectoryEntry targetDir) {
            if (targetDir.Exists) { throw new IOException("Target dir to extract zip into already exists: " + targetDir); }
            var fastZip = new FastZip();
            FastZip.ConfirmOverwriteDelegate confCallback = (fileName) => false;
            using (var s = self.Open(FileMode.Open, FileAccess.Read, FileShare.Read)) {
                fastZip.ExtractZip(s, GetFullFileSystemPath(targetDir), FastZip.Overwrite.Prompt, confCallback, "", "", true, true);
            }
        }

        public static DirectoryEntry GetRootDirectory(this ZipArchiveFileSystem self) {
            return new DirectoryEntry(self, UPath.Root);
        }

        [Obsolete("Use file.OpenOrCreateAsZip() instead")]
        public static ZipArchiveFileSystem OpenAsZip(this FileEntry self) {
            return OpenOrCreateAsZip(self);
        }

        public static ZipArchiveFileSystem OpenOrCreateAsZip(this FileEntry self) {
            if (self.Parent != null && !self.Parent.Exists) {
                throw new IOException("Parent directory does not exist: " + self.Parent);
            }
            var openedOrCreatedStream = self.OpenOrCreateForReadWrite();
            return new ZipArchiveFileSystem(openedOrCreatedStream);
        }

        /// <summary> Currently only works when working with a physical file system for the source directory </summary>
        public static void ZipToFile(this DirectoryEntry self, FileEntry targetZipFile) {
            if (targetZipFile.Exists) { throw new IOException("Target zip file already exists: " + targetZipFile); }
            var fastZip = new FastZip();
            fastZip.CreateZip(targetZipFile.Create(), GetFullFileSystemPath(self), true, "", "");
        }

        public static string GetFullFileSystemPath(this FileSystemEntry f) {
            return f.FileSystem.ConvertPathToInternal(f.Path);
        }

        public static DateTime LastWriteTimeUtc(this FileSystemEntry targetFile) {
            return targetFile.LastWriteTime.ToUniversalTime();
        }

        public static void SetLastWriteTimeUtc(this FileSystemEntry targetFile, DateTime utcTimestamp) {
            targetFile.LastWriteTime = utcTimestamp.ToLocalTime();
        }

        public static string CalcFileMd5Hash(this FileEntry targetFile) {
            using (var fileStream = targetFile.OpenForRead()) { return fileStream.GetMD5Hash(); }
        }

        public static bool HasExtension(this FileEntry self) { return self.NameWithoutExtension != self.Name; }

        public static string ExtensionWithoutDot(this FileEntry self) {
            var ext = self.ExtensionWithDot;
            return ext != null && ext.StartsWith(".") ? ext.Substring(1) : ext;
        }

        /// <summary> Returns an URI string that starts with file://.. </summary>
        public static string GetFileUri(this FileSystemEntry self) {

            // Does not handle % encoding correctly (e.g. 'C:\%51.txt' should become 'file:///C:/%2551.txt')
            // return new Uri(self.GetFullFileSystemPath(), UriKind.Absolute).ToString();

            // From https://stackoverflow.com/a/35734486/165106
            var filePath = self.GetFullFileSystemPath();
            StringBuilder uri = new StringBuilder();
            foreach (char v in filePath) {
                if ((v >= 'a' && v <= 'z') || (v >= 'A' && v <= 'Z') || (v >= '0' && v <= '9') ||
                    v == '+' || v == '/' || v == ':' || v == '.' || v == '-' || v == '_' || v == '~' ||
                    v > '\xFF') {
                    uri.Append(v);
                } else if (v == Path.DirectorySeparatorChar || v == Path.AltDirectorySeparatorChar) {
                    uri.Append('/');
                } else {
                    uri.Append(String.Format("%{0:X2}", (int)v));
                }
            }
            if (uri.Length >= 2 && uri[0] == '/' && uri[1] == '/') // UNC path
                uri.Insert(0, "file:");
            else
                uri.Insert(0, "file:///");
            return uri.ToString();
        }

    }

}