using com.csutil.http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zio;

namespace com.csutil.model {

    public static class FileRefHelper {

        public const string CHECKSUM_MD5 = "md5";
        public const string CHECKSUM_ETAG = "etag";

        public static UPath GetPath(this FileRef self) { return (UPath)self.dir / self.fileName; }

        public static void SetPath(this FileRef self, FileEntry file) {
            UPath value = file.Path;
            self.dir = "" + value.GetDirectory();
            self.fileName = value.GetName();
        }

        public static async Task<bool> DownloadTo(this FileRef self, DirectoryEntry targetDirectory, Action<float> onProgress = null) {
            self.SetTargetDir(targetDirectory);
            RestRequest request = new Uri(self.url).SendGET();
            if (onProgress != null) { request.onProgress = onProgress; }
            return await self.DownloadTo(request, targetDirectory);
        }

        public static async Task<bool> DownloadTo(this FileRef self, RestRequest request, DirectoryEntry targetDir) {
            var targetFile = targetDir.GetChild(CalculateFileName(self, await request.GetResultHeaders()));
            return await self.DownloadTo(request, targetFile);
        }

        public static async Task<bool> DownloadTo(this FileRef self, RestRequest request, FileEntry targetFile) {
            var headers = await request.GetResultHeaders();
            if (self.IsAlreadyDownloaded(headers, targetFile)) { return false; }
            await request.DownloadTo(targetFile);
            self.CheckMD5AfterDownload(headers, targetFile);
            self.SetLocalFileInfosFrom(headers, targetFile);
            return true;
        }

        private static bool IsAlreadyDownloaded(this FileRef self, Headers headers, FileEntry targetFile) {
            if (targetFile.Exists) {
                // Cancel download if etag header matches the locally stored one:
                if (self.HasMatchingChecksum(headers.GetEtagHeader())) { return true; }
                // Cancel download if local file with the same MD5 hash exists:
                var onlineMD5 = headers.GetMD5Checksum();
                if (!onlineMD5.IsNullOrEmpty()) {
                    if (self.HasMatchingChecksum(onlineMD5)) { return true; }
                    if (onlineMD5 == CalcLocalMd5Hash(targetFile)) { return true; }
                }
                // Cancel download if local file with the exact last-write timestamp exists:
                if (headers.GetRawLastModifiedString() != null) {
                    var distance = headers.GetLastModifiedUtcDate(DateTime.MinValue) - targetFile.LastWriteTime.ToUniversalTime();
                    Log.d("distance.Milliseconds: " + distance.Milliseconds);
                    if (distance.Milliseconds == 0) { return true; }
                }
            }
            return false;
        }

        private static bool HasMatchingChecksum(this FileRef self, string hash) {
            return !hash.IsNullOrEmpty() && self.checksums != null && self.checksums.Any(x => {
                return hash.Equals(x.Value);
            });
        }

        private static void AddCheckSum(this FileRef self, string type, string hash) {
            if (hash == null) { throw new ArgumentNullException($"The passed {type}-hash was null"); }
            if (self.checksums == null) { self.checksums = new Dictionary<string, object>(); }
            self.checksums.Add(type, hash);
        }

        private static void SetLocalFileInfosFrom(this FileRef self, Headers headers, FileEntry targetFile) {
            self.fileName = targetFile.Name;
            self.SetTargetDir(targetFile.Parent);
            self.mimeType = headers.GetContentMimeType(null);
            if (headers.GetRawLastModifiedString() != null) {
                targetFile.LastWriteTime = headers.GetLastModifiedUtcDate(DateTime.MinValue);
            }
            if (headers.GetEtagHeader() != null) { self.AddCheckSum(CHECKSUM_ETAG, headers.GetEtagHeader()); }
        }

        private static void SetTargetDir(this FileRef self, DirectoryEntry targetDirectory) {
            if (!targetDirectory.IsNotNullAndExists()) {
                throw new ArgumentException("Cant download into non existing directory=" + targetDirectory);
            }
            if (self.dir != null && targetDirectory.FullName != self.dir) {
                throw new ArgumentException($"Dir already set, wont change from '{self.dir}' to '{targetDirectory}'");
            }
            self.dir = targetDirectory.FullName;
        }

        private static string CalculateFileName(FileRef self, Headers headers) {
            if (!self.fileName.IsNullOrEmpty()) { return self.fileName; }
            var nameOnServer = headers.GetFileNameOnServer();
            if (!nameOnServer.IsNullOrEmpty()) { return nameOnServer; }
            var hashedHeaders = headers.GenerateHashNameFromHeaders();
            if (!hashedHeaders.IsNullOrEmpty()) { return hashedHeaders; }
            return self.url.GetMD5Hash();
        }

        private static bool CheckMD5AfterDownload(this FileRef self, Headers headers, FileEntry targetFile) {
            var onlineMD5 = headers.GetMD5Checksum();
            if (onlineMD5.IsNullOrEmpty()) { return false; }
            string localMD5 = CalcLocalMd5Hash(targetFile);
            if (localMD5 == onlineMD5) {
                throw new InvalidDataException($"Missmatch in MD5 hashes, local={localMD5} & online={onlineMD5}");
            }
            self.AddCheckSum(CHECKSUM_MD5, localMD5);
            return true;
        }

        private static string CalcLocalMd5Hash(FileEntry targetFile) {
            using (var fileStream = targetFile.OpenForRead()) { return fileStream.GetMD5Hash(); }
        }
    }

}
