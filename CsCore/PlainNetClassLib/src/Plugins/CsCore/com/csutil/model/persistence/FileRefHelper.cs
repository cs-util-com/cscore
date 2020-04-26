using com.csutil.datastructures;
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

        public static void AddCheckSum(this FileRef self, string type, string hash) {
            if (hash == null) { throw new ArgumentNullException($"The passed {type}-hash was null"); }
            if (self.checksums == null) { self.checksums = new Dictionary<string, object>(); }
            self.checksums.Add(type, hash);
        }

        public static bool HasMatchingChecksum(this FileRef self, string hash) {
            return !hash.IsNullOrEmpty() && self.checksums != null && self.checksums.Any(x => {

                return hash.Equals(x.Value);
            });
        }

        public static async Task<bool> DownloadTo(this FileRef self, DirectoryEntry targetDirectory) {
            if (!targetDirectory.IsNotNullAndExists()) {
                throw new ArgumentException("Cant download into non existing directory=" + targetDirectory);
            }

            var request = new Uri(self.url).SendGET();
            var headers = await request.GetResultHeaders();
            var targetFile = targetDirectory.GetChild(CalculateFileName(self, headers));
            if (targetFile.Exists) {

                // Cancel download if etag header matches the locally stored one:
                if (self.HasMatchingChecksum(headers.GetEtagHeader())) { return false; }

                // Cancel download if local file with the same MD5 hash exists:
                var onlineMD5 = headers.GetMD5Checksum();
                if (!onlineMD5.IsNullOrEmpty()) {
                    if (self.HasMatchingChecksum(onlineMD5)) { return false; }
                    if (onlineMD5 == CalcLocalMd5Hash(targetFile)) { return false; }
                }

                // Cancel download if local file with the exact last-write timestamp exists:
                if (headers.GetRawLastModifiedString() != null) {
                    var distance = headers.GetLastModifiedUtcDate(DateTime.MinValue) - targetFile.LastWriteTime.ToUniversalTime();
                    Log.d("distance.Milliseconds: " + distance.Milliseconds);
                    if (distance.Milliseconds == 0) { return false; }
                }

            }
            using (var stream = await request.GetResult<Stream>()) {

                float totalBytes = headers.GetFileSizeInBytesOnServer();
                var progressInPercent = new ChangeTracker<float>(0);
                await targetFile.SaveStreamAsync(stream, (savedBytes) => {
                    if (progressInPercent.setNewValue(100 * savedBytes / totalBytes)) {
                        request.onProgress?.Invoke(progressInPercent.value);
                    }
                });

                self.CheckMD5AfterDownload(targetFile, headers);
                self.fileName = targetFile.Name;
                self.dir = targetDirectory.FullName;
                self.mimeType = headers.GetContentMimeType(null);
                if (headers.GetRawLastModifiedString() != null) {
                    targetFile.LastWriteTime = headers.GetLastModifiedUtcDate(DateTime.MinValue);
                }
                if (headers.GetEtagHeader() != null) { self.AddCheckSum(CHECKSUM_ETAG, headers.GetEtagHeader()); }
            }

            return true;
        }

        private static string CalculateFileName(FileRef self, Headers headers) {
            if (!self.fileName.IsNullOrEmpty()) { return self.fileName; }
            var nameOnServer = headers.GetFileNameOnServer();
            if (!nameOnServer.IsNullOrEmpty()) { return nameOnServer; }
            var hashedHeaders = headers.GenerateHashNameFromHeaders();
            if (!hashedHeaders.IsNullOrEmpty()) { return hashedHeaders; }
            return self.url.GetMD5Hash();
        }

        private static bool CheckMD5AfterDownload(this FileRef self, FileEntry targetFile, Headers headers) {
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
