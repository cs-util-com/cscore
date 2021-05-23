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

        public static UPath GetPath(this IFileRef self) { return (UPath)self.dir / self.fileName; }

        public static void SetPath(this IFileRef self, FileEntry file) {
            UPath value = file.Path;
            self.dir = "" + value.GetDirectory();
            self.fileName = value.GetName();
        }

        public static DirectoryEntry GetDirectoryEntry(this IFileRef self, IFileSystem fs) {
            if (self.dir.IsNullOrEmpty()) { throw new InvalidDataException("IFileRef.dir not set"); }
            return new DirectoryEntry(fs, self.dir);
        }

        public static FileEntry GetFileEntry(this IFileRef self, IFileSystem fs) {
            return self.GetDirectoryEntry(fs).GetChild(self.fileName);
        }

        public static async Task<bool> DownloadTo(this IFileRef self, DirectoryEntry targetDirectory, Action<float> onProgress = null, bool useAutoCachedFileRef = false) {
            self.AssertValidDirectory(targetDirectory);
            FileEntry cachedFileRef = self.LoadAutoCachedFileRef(targetDirectory, useAutoCachedFileRef);
            RestRequest request = new Uri(self.url).SendGET();
            if (onProgress != null) { request.onProgress = onProgress; }
            bool downloadWasNeeded = await self.DownloadTo(request, targetDirectory);
            if (useAutoCachedFileRef) { cachedFileRef.SaveAsJson(self, true); }
            return downloadWasNeeded;
        }

        private static FileEntry LoadAutoCachedFileRef(this IFileRef self, DirectoryEntry targetDirectory, bool useAutoCachedFileRef) {
            self.url.ThrowErrorIfNullOrEmpty("IFileRef.url");
            var cachedFile = targetDirectory.GetChild("fileRef-" + self.url.GetMD5Hash() + ".json");
            if (cachedFile.IsNotNullAndExists()) {
                if (!useAutoCachedFileRef) {
                    Log.w("A file exists that seems to contain the fileRef details but it will not be used");
                    return null;
                }
                FillFileRefValuesFrom(cachedFile, self);
            }
            return cachedFile;
        }

        private static void FillFileRefValuesFrom(FileEntry source, IFileRef targetToFill) {
            try {
                var loaded = source.LoadAs(targetToFill.GetType()) as IFileRef;
                if (targetToFill.dir.IsNullOrEmpty()) { targetToFill.dir = loaded.dir; }
                if (targetToFill.fileName.IsNullOrEmpty()) { targetToFill.fileName = loaded.fileName; }
                if (targetToFill.checksums.IsNullOrEmpty()) { targetToFill.checksums = loaded.checksums; }
                if (targetToFill.mimeType.IsNullOrEmpty()) { targetToFill.mimeType = loaded.mimeType; }
            }
            catch (Exception e) { Log.e(e); }
        }

        public static async Task<bool> DownloadTo(this IFileRef self, RestRequest request, DirectoryEntry targetDir, int maxNrOfRetries = 4) {
            self.AssertValidDirectory(targetDir);
            var fileName = CalculateFileName(self, await request.GetResultHeaders());
            var targetFile = targetDir.GetChild(fileName);
            bool downloadWasNeeded = await TaskV2.TryWithExponentialBackoff(async () => {
                return await self.DownloadTo(request, targetFile);
            }, maxNrOfRetries: maxNrOfRetries, initialExponent: 10);
            if (self.fileName.IsNullOrEmpty() && targetFile.Exists) {
                throw new MissingFieldException("IFileRef.fileName not set after successful download, must not happen!");
            }
            return downloadWasNeeded;
        }

        public static async Task<bool> DownloadTo(this IFileRef self, RestRequest request, FileEntry targetFile) {
            var headers = await request.GetResultHeaders();
            var isAlreadyDownloaded = self.IsAlreadyDownloaded(headers, targetFile);
            if (!isAlreadyDownloaded) {
                await request.DownloadTo(targetFile);
                self.CheckMD5AfterDownload(headers, targetFile);
            }
            self.SetLocalFileInfosFrom(headers, targetFile);
            return !isAlreadyDownloaded;
        }

        private static bool IsAlreadyDownloaded(this IFileRef self, Headers headers, FileEntry targetFile) {
            if (targetFile.Exists) {
                AssertV2.IsFalse(self.checksums.IsNullOrEmpty(), "targetFile.Exists but no checksums stored: " + self.url);
                // Cancel download if etag header matches the locally stored one:
                if (self.HasMatchingChecksum(headers.GetEtagHeader())) { return true; }
                // Cancel download if local file with the same MD5 hash exists:
                var onlineMD5 = headers.GetMD5Checksum();
                if (!onlineMD5.IsNullOrEmpty()) {
                    if (self.HasMatchingChecksum(onlineMD5)) { return true; }
                    if (onlineMD5 == targetFile.CalcFileMd5Hash()) { return true; }
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

        private static bool HasMatchingChecksum(this IFileRef self, string hash) {
            return !hash.IsNullOrEmpty() && self.checksums != null && self.checksums.Any(x => {
                return hash.Equals(x.Value);
            });
        }

        private static void AddCheckSum(this IFileRef self, string type, string hash) {
            hash.ThrowErrorIfNull($"The passed {type}-hash was null");
            if (self.checksums == null) { self.checksums = new Dictionary<string, object>(); }
            self.checksums.AddOrReplace(type, hash);
        }

        private static void SetLocalFileInfosFrom(this IFileRef self, Headers headers, FileEntry targetFile) {
            self.AssertValidDirectory(targetFile.Parent);
            self.dir = targetFile.Parent.FullName;
            self.fileName = targetFile.Name;
            self.mimeType = headers.GetContentMimeType(null);
            if (headers.GetRawLastModifiedString() != null) {
                targetFile.LastWriteTime = headers.GetLastModifiedUtcDate(DateTime.MinValue);
            }
            if (headers.GetEtagHeader() != null) { self.AddCheckSum(CHECKSUM_ETAG, headers.GetEtagHeader()); }
        }

        private static void AssertValidDirectory(this IFileRef self, DirectoryEntry targetDirectory) {
            if (!targetDirectory.IsNotNullAndExists()) {
                throw new ArgumentException("Cant download into non existing directory=" + targetDirectory);
            }
            if (self.dir != null && targetDirectory.FullName != self.dir) {
                throw new ArgumentException($"Dir already set, wont change from '{self.dir}' to '{targetDirectory}'");
            }
        }

        private static string CalculateFileName(IFileRef self, Headers headers) {
            if (!self.fileName.IsNullOrEmpty()) { return self.fileName; }
            var nameOnServer = headers.GetFileNameOnServer();
            if (!nameOnServer.IsNullOrEmpty()) { return nameOnServer; }
            var hashedHeaders = headers.GenerateHashNameFromHeaders();
            if (!hashedHeaders.IsNullOrEmpty()) { return hashedHeaders; }
            return self.url.GetMD5Hash();
        }

        private static bool CheckMD5AfterDownload(this IFileRef self, Headers headers, FileEntry targetFile) {
            var onlineMD5 = headers.GetMD5Checksum();
            if (onlineMD5.IsNullOrEmpty()) { return false; }
            string localMD5 = targetFile.CalcFileMd5Hash();
            if (localMD5 != onlineMD5) {
                throw new InvalidDataException($"Missmatch in MD5 hashes, local={localMD5} & online={onlineMD5}");
            }
            self.AddCheckSum(CHECKSUM_MD5, localMD5);
            return true;
        }

        public static bool IsJpgFile(this IFileRef self) {
            if (!self.mimeType.IsNullOrEmpty()) {
                return self.mimeType == "image/jpeg";
            }
            if (!self.fileName.IsNullOrEmpty()) {
                bool endsWithJpg = self.fileName.EndsWith(".jpg");
                bool endsWithJpeg = self.fileName.EndsWith(".jpeg");
                return endsWithJpg || endsWithJpeg;
            }
            Log.e("Could not resolve if image is of type jpg");
            return false;
        }

    }

}
