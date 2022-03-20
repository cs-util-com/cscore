// Modified version of the original Zio.FileSystems.PhysicalFileSystem

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ZetaLongPaths;

namespace Zio.FileSystems {

    /// <summary>
    /// Internally uses ZetaLongPaths library
    /// </summary>
    public class PhysicalFileSystemUnityWindows : FileSystem {

        /// <summary> Can contain the disc name (e.g. C:) </summary>
        public readonly string discPrefix;
        private int discPrefixLenth = 0;

        public PhysicalFileSystemUnityWindows(string discPrefix) {
            this.discPrefix = discPrefix;
            if (discPrefix != null) { this.discPrefixLenth = discPrefix.Length; }
        }

        protected override UPath ConvertPathFromInternalImpl(string innerPath) {
            if (discPrefixLenth <= 0) { return innerPath; }
            AssertStartsWithDiscPrefix(innerPath);
            return innerPath.Substring(discPrefixLenth);
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        private void AssertStartsWithDiscPrefix(string path) {
            if (!path.StartsWith(discPrefix)) {
                throw new ArgumentException("Path did not start with disc prefix: " + path);
            }
        }

        protected override string ConvertPathToInternalImpl(UPath path) {
            if (discPrefixLenth <= 0) { return path.FullName; }
            return discPrefix + path.FullName;
        }

        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite) {
            ZlpIOHelper.CopyFile(ConvertPathToInternal(srcPath), ConvertPathToInternal(destPath), overwrite);
        }

        protected override void CreateDirectoryImpl(UPath path) {
            ZlpIOHelper.CreateDirectory(ConvertPathToInternal(path));
        }

        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive) {
            throw new NotImplementedException();
        }

        protected override void DeleteFileImpl(UPath path) {
            throw new NotImplementedException();
        }

        protected override bool DirectoryExistsImpl(UPath path) {
            throw new NotImplementedException();
        }

        protected override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget) {
            throw new NotImplementedException();
        }

        protected override bool FileExistsImpl(UPath path) {
            throw new NotImplementedException();
        }

        protected override FileAttributes GetAttributesImpl(UPath path) {
            throw new NotImplementedException();
        }

        protected override DateTime GetCreationTimeImpl(UPath path) {
            throw new NotImplementedException();
        }

        protected override long GetFileLengthImpl(UPath path) {
            throw new NotImplementedException();
        }

        protected override DateTime GetLastAccessTimeImpl(UPath path) {
            throw new NotImplementedException();
        }

        protected override DateTime GetLastWriteTimeImpl(UPath path) {
            throw new NotImplementedException();
        }

        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath) {
            throw new NotImplementedException();
        }

        protected override void MoveFileImpl(UPath srcPath, UPath destPath) {
            throw new NotImplementedException();
        }

        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share) {
            throw new NotImplementedException();
        }

        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors) {
            throw new NotImplementedException();
        }

        protected override void SetAttributesImpl(UPath path, FileAttributes attributes) {
            throw new NotImplementedException();
        }

        protected override void SetCreationTimeImpl(UPath path, DateTime time) {
            throw new NotImplementedException();
        }

        protected override void SetLastAccessTimeImpl(UPath path, DateTime time) {
            throw new NotImplementedException();
        }

        protected override void SetLastWriteTimeImpl(UPath path, DateTime time) {
            throw new NotImplementedException();
        }

        protected override IFileSystemWatcher WatchImpl(UPath path) {
            throw new NotImplementedException();
        }

    }

}