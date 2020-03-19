// Modified version of the original Zio.FileSystems.PhysicalFileSystem

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Zio.FileSystems {

    /// <summary> Provides a <see cref="IFileSystem"/> for the physical filesystem. </summary>
    public class PhysicalFileSystemV2 : FileSystem {

        /// <summary> Can contain the disc name (e.g. C:) </summary>
        public readonly string discPrefix;
        private int discPrefixLenth = 0;

        public PhysicalFileSystemV2(string discPrefix) {
            this.discPrefix = discPrefix;
            if (discPrefix != null) { this.discPrefixLenth = discPrefix.Length; }
        }

        // ----------------------------------------------
        // Directory API
        // ----------------------------------------------

        /// <inheritdoc />
        protected override void CreateDirectoryImpl(UPath path) {
            Directory.CreateDirectory(ConvertPathToInternal(path));
        }

        /// <inheritdoc />
        protected override bool DirectoryExistsImpl(UPath path) {
            var pp = ConvertPathToInternal(path);
            return Directory.Exists(pp);
        }

        /// <inheritdoc />
        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath) {
            var systemSrcPath = ConvertPathToInternal(srcPath);
            var systemDestPath = ConvertPathToInternal(destPath);

            // If the souce path is a file
            var fileInfo = new FileInfo(systemSrcPath);
            if (fileInfo.Exists) {
                throw new IOException($"The source `{srcPath}` is not a directory");
            }

            Directory.Move(systemSrcPath, systemDestPath);
        }

        /// <inheritdoc />
        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive) {
            Directory.Delete(ConvertPathToInternal(path), isRecursive);
        }

        // ----------------------------------------------
        // File API
        // ----------------------------------------------

        /// <inheritdoc />
        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite) {
            File.Copy(ConvertPathToInternal(srcPath), ConvertPathToInternal(destPath), overwrite);
        }

        /// <inheritdoc />
        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors) {
            if (!destBackupPath.IsNull) {
                CopyFileImpl(destPath, destBackupPath, true);
            }
            CopyFileImpl(srcPath, destPath, true);
            DeleteFileImpl(srcPath);

            // TODO: Add atomic version using File.Replace coming with .NET Standard 2.0
        }

        /// <inheritdoc />
        protected override long GetFileLengthImpl(UPath path) {
            return new FileInfo(ConvertPathToInternal(path)).Length;
        }

        /// <inheritdoc />
        protected override bool FileExistsImpl(UPath path) {
            return File.Exists(ConvertPathToInternal(path));
        }

        /// <inheritdoc />
        protected override void MoveFileImpl(UPath srcPath, UPath destPath) {
            File.Move(ConvertPathToInternal(srcPath), ConvertPathToInternal(destPath));
        }

        /// <inheritdoc />
        protected override void DeleteFileImpl(UPath path) {
            File.Delete(ConvertPathToInternal(path));
        }

        /// <inheritdoc />
        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access,
            FileShare share = FileShare.None) {
            return File.Open(ConvertPathToInternal(path), mode, access, share);
        }

        /// <inheritdoc />
        protected override FileAttributes GetAttributesImpl(UPath path) {
            return File.GetAttributes(ConvertPathToInternal(path));
        }

        // ----------------------------------------------
        // Metadata API
        // ----------------------------------------------

        /// <inheritdoc />
        protected override void SetAttributesImpl(UPath path, FileAttributes attributes) {
            File.SetAttributes(ConvertPathToInternal(path), attributes);
        }

        /// <inheritdoc />
        protected override DateTime GetCreationTimeImpl(UPath path) {
            return File.GetCreationTime(ConvertPathToInternal(path));
        }

        /// <inheritdoc />
        protected override void SetCreationTimeImpl(UPath path, DateTime time) {
            File.SetCreationTime(ConvertPathToInternal(path), time);
        }

        /// <inheritdoc />
        protected override DateTime GetLastAccessTimeImpl(UPath path) {
            return File.GetLastAccessTime(ConvertPathToInternal(path));
        }

        /// <inheritdoc />
        protected override void SetLastAccessTimeImpl(UPath path, DateTime time) {
            File.SetLastAccessTime(ConvertPathToInternal(path), time);
        }

        /// <inheritdoc />
        protected override DateTime GetLastWriteTimeImpl(UPath path) {
            return File.GetLastWriteTime(ConvertPathToInternal(path));
        }

        /// <inheritdoc />
        protected override void SetLastWriteTimeImpl(UPath path, DateTime time) {
            File.SetLastWriteTime(ConvertPathToInternal(path), time);
        }

        // ----------------------------------------------
        // Search API
        // ----------------------------------------------

        /// <inheritdoc />
        protected override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget) {
            var search = SearchPattern.Parse(ref path, ref searchPattern);

            IEnumerable<string> results;
            switch (searchTarget) {
                case SearchTarget.File:
                    results = Directory.EnumerateFiles(ConvertPathToInternal(path), searchPattern, searchOption);
                    break;

                case SearchTarget.Directory:
                    results = Directory.EnumerateDirectories(ConvertPathToInternal(path), searchPattern, searchOption);
                    break;

                case SearchTarget.Both:
                    results = Directory.EnumerateFileSystemEntries(ConvertPathToInternal(path), searchPattern, searchOption);
                    break;

                default:
                    yield break;
            }

            foreach (var subPath in results) {
                // Windows will truncate the search pattern's extension to three characters if the filesystem
                // has 8.3 paths enabled. This means searching for *.docx will list *.doc as well which is
                // not what we want. Check against the search pattern again to filter out those false results.
                if (search.Match(Path.GetFileName(subPath))) {
                    yield return ConvertPathFromInternal(subPath);
                }
            }
        }

        // ----------------------------------------------
        // Watch API
        // ----------------------------------------------

        /// <inheritdoc />
        protected override bool CanWatchImpl(UPath path) {
            return Directory.Exists(ConvertPathToInternal(path));
        }

        /// <inheritdoc />
        protected override IFileSystemWatcher WatchImpl(UPath path) {
            return new Watcher(this, path);
        }

        private class Watcher : IFileSystemWatcher {
            private readonly PhysicalFileSystemV2 _fileSystem;
            private readonly System.IO.FileSystemWatcher _watcher;

            public event EventHandler<FileChangedEventArgs> Changed;
            public event EventHandler<FileChangedEventArgs> Created;
            public event EventHandler<FileChangedEventArgs> Deleted;
            public event EventHandler<FileSystemErrorEventArgs> Error;
            public event EventHandler<FileRenamedEventArgs> Renamed;

            public IFileSystem FileSystem => _fileSystem;
            public UPath Path { get; }

            public int InternalBufferSize {
                get => _watcher.InternalBufferSize;
                set => _watcher.InternalBufferSize = value;
            }

            public NotifyFilters NotifyFilter {
                get => (NotifyFilters)_watcher.NotifyFilter;
                set => _watcher.NotifyFilter = (System.IO.NotifyFilters)value;
            }

            public bool EnableRaisingEvents {
                get => _watcher.EnableRaisingEvents;
                set => _watcher.EnableRaisingEvents = value;
            }

            public string Filter {
                get => _watcher.Filter;
                set => _watcher.Filter = value;
            }

            public bool IncludeSubdirectories {
                get => _watcher.IncludeSubdirectories;
                set => _watcher.IncludeSubdirectories = value;
            }

            public Watcher(PhysicalFileSystemV2 fileSystem, UPath path) {
                _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
                _watcher = new System.IO.FileSystemWatcher(_fileSystem.ConvertPathToInternal(path)) {
                    Filter = "*"
                };
                Path = path;

                _watcher.Changed += (sender, args) => Changed?.Invoke(this, Remap(args));
                _watcher.Created += (sender, args) => Created?.Invoke(this, Remap(args));
                _watcher.Deleted += (sender, args) => Deleted?.Invoke(this, Remap(args));
                _watcher.Error += (sender, args) => Error?.Invoke(this, Remap(args));
                _watcher.Renamed += (sender, args) => Renamed?.Invoke(this, Remap(args));
            }

            ~Watcher() {
                Dispose(false);
            }

            public void Dispose() {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing) {
                if (disposing) {
                    _watcher.Dispose();
                }
            }

            private FileChangedEventArgs Remap(FileSystemEventArgs args) {
                var newChangeType = (WatcherChangeTypes)args.ChangeType;
                var newPath = _fileSystem.ConvertPathFromInternal(args.FullPath);
                return new FileChangedEventArgs(FileSystem, newChangeType, newPath);
            }

            private FileSystemErrorEventArgs Remap(ErrorEventArgs args) {
                return new FileSystemErrorEventArgs(args.GetException());
            }

            private FileRenamedEventArgs Remap(RenamedEventArgs args) {
                var newChangeType = (WatcherChangeTypes)args.ChangeType;
                var newPath = _fileSystem.ConvertPathFromInternal(args.FullPath);
                var newOldPath = _fileSystem.ConvertPathFromInternal(args.OldFullPath);
                return new FileRenamedEventArgs(FileSystem, newChangeType, newPath, newOldPath);
            }
        }

        // ----------------------------------------------
        // Path API
        // ----------------------------------------------

        /// <inheritdoc />
        protected override string ConvertPathToInternalImpl(UPath path) {
            if (discPrefixLenth <= 0) { return path.FullName; }
            return discPrefix + path.FullName;
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        private void AssertStartsWithDiscPrefix(string path) {
            if (!path.StartsWith(discPrefix)) {
                throw new ArgumentException("Path did not start with disc prefix: " + path);
            }
        }

        /// <inheritdoc />
        protected override UPath ConvertPathFromInternalImpl(string innerPath) {
            if (discPrefixLenth <= 0) { return innerPath; }
            AssertStartsWithDiscPrefix(innerPath);
            return innerPath.Substring(discPrefixLenth);
        }

    }

}