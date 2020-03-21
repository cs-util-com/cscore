using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Zio;
using Zio.FileSystems;

namespace com.csutil.io {

    public class ZipFileReadSystem : FileSystem {

        private readonly ZipFile zip;

        public ZipFileReadSystem(ZipFile zip) {
            this.zip = zip;
        }

        protected override void Dispose(bool disposing) {
            zip.Close();
        }

        protected override UPath ConvertPathFromInternalImpl(string innerPath) {
            return innerPath;
        }

        protected override string ConvertPathToInternalImpl(UPath path) {
            return path.FullName;
        }

        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite) {
            throw new NotImplementedException();
        }

        protected override void CreateDirectoryImpl(UPath path) {
            throw new NotImplementedException();
        }

        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive) {
            throw new NotImplementedException();
        }

        protected override void DeleteFileImpl(UPath path) {
            throw new NotImplementedException();
        }

        protected override bool DirectoryExistsImpl(UPath path) {
            if (path == UPath.Root) { return true; }
            return GetChildrenOf(path).Any();
        }

        private IEnumerable<UPath> GetChildrenOf(UPath path) {
            return EnumeratePathsImpl(path, "*", SearchOption.TopDirectoryOnly, SearchTarget.Both);
        }

        protected override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget) {
            var search = SearchPattern.Parse(ref path, ref searchPattern);
            return zip.Cast<ZipEntry>().Map(x => {
                if (searchTarget == SearchTarget.File && !x.IsFile) { return null; }
                if (searchTarget == SearchTarget.Directory && !x.IsDirectory) { return null; }
                UPath child = UPath.DirectorySeparator + x.Name;
                if (!child.FullName.StartsWith(path.FullName)) { return null; }
                if (searchOption == SearchOption.TopDirectoryOnly) { child = child.RemovePrefix(path).GetFirstPart(); }
                if (!search.Match(child)) { return null; }
                return child;
            }).Filter(x => x != null).Distinct();
        }

        protected override bool FileExistsImpl(UPath path) {
            return GetZipEntry(path) != null;
        }

        private ZipEntry GetZipEntry(UPath path) { return zip.GetEntry(path.FullName); }

        protected override FileAttributes GetAttributesImpl(UPath path) {
            return NewFileAttributesFrom(GetZipEntry(path));
        }

        private FileAttributes NewFileAttributesFrom(ZipEntry zipEntry) {
            var result = FileAttributes.ReadOnly | FileAttributes.Compressed;
            if (zipEntry.IsDirectory) { result |= FileAttributes.Directory; }
            if (zipEntry.IsCrypted) { result |= FileAttributes.Encrypted; }
            return result;
        }

        protected override DateTime GetCreationTimeImpl(UPath path) {
            return GetZipEntry(path).DateTime;
        }

        protected override long GetFileLengthImpl(UPath path) {
            return GetZipEntry(path).Size;
        }

        protected override DateTime GetLastAccessTimeImpl(UPath path) {
            throw new NotImplementedException();
        }

        protected override DateTime GetLastWriteTimeImpl(UPath path) {
            return GetZipEntry(path).DateTime;
        }

        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath) {
            throw new NotImplementedException();
        }

        protected override void MoveFileImpl(UPath srcPath, UPath destPath) {
            throw new NotImplementedException();
        }

        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share) {
            if (mode == FileMode.Open) {
                var entry = zip.GetEntry(path.ToRelative().FullName);
                AssertV2.IsTrue(entry != null && entry.IsFile, "Not a file: " + entry);
                return zip.GetInputStream(entry);
            }
            throw new NotImplementedException("Only read via FileMode.Open supported!");
            // var stream = new ZipOutputStream(File.Open(zip.Name, mode, access, share));
            // stream.PutNextEntry(new ZipEntry(path.ToRelative().FullName));
            // return stream;
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