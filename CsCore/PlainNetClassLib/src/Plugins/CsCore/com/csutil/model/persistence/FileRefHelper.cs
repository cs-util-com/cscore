using com.csutil.http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="targetDirectory"></param>
        /// <param name="onProgress"> A float from 0 to 100 </param>
        /// <param name="useAutoCachedFileRef"> If true the fileRef will create itself a cache file in the targetDirectory </param>
        /// <returns></returns>
        public static async Task<bool> DownloadTo(this IFileRef self, DirectoryEntry targetDirectory, Action<float> onProgress = null, bool useAutoCachedFileRef = false, int maxNrOfRetries = 4) {
            self.AssertValidDirectory(targetDirectory);
            FileEntry cachedFileRef = self.LoadAutoCachedFileRef(targetDirectory, useAutoCachedFileRef);
            AssertIsValidUrl(self.url);
            RestRequest request = new Uri(self.url).SendGET();
            if (onProgress != null) { request.onProgress = onProgress; }
            bool downloadWasNeeded = await self.DownloadTo(request, targetDirectory, maxNrOfRetries);
            if (useAutoCachedFileRef) { cachedFileRef.SaveAsJson(self, true); }
            return downloadWasNeeded;
        }

        [Conditional("DEBUG")]
        private static void AssertIsValidUrl(string url) {
            if (url.IsNullOrEmpty()) {
                Log.e("IFileRef.url not set");
            } else {
                try {
                    var uri = new Uri(url);
                } catch (Exception e) {
                    Log.e($"IFileRef.url is not a valid url: '{url}'", e);
                }
            }
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
            } catch (Exception e) { Log.e(e); }
        }

        public static async Task<bool> DownloadTo(this IFileRef self, RestRequest request, DirectoryEntry targetDir, int maxNrOfRetries) {
            self.AssertValidDirectory(targetDir);
            var fileName = await CalculateFileName(self, request);
            var targetFile = targetDir.GetChild(fileName);
            bool downloadWasNeeded;
            if (maxNrOfRetries == 0) {
                downloadWasNeeded = await self.DownloadTo(request, targetFile);
            } else {
                int initialExponent = 9; // 2^9 = 512ms (for first delay if fails)
                downloadWasNeeded = await TaskV2.TryWithExponentialBackoff(async () => {
                    return await self.DownloadTo(request, targetFile);
                }, maxNrOfRetries: maxNrOfRetries, initialExponent: initialExponent);
            }
            if (self.fileName.IsNullOrEmpty() && targetFile.Exists) {
                throw new MissingFieldException("IFileRef.fileName not set after successful download, must not happen!");
            }
            return downloadWasNeeded;
        }

        public static async Task<bool> DownloadTo(this IFileRef self, RestRequest request, FileEntry targetFile) {
            if (!await InternetStateManager.Instance(self).HasInetAsync && targetFile.Exists) {
                self.SetLocalFileInfosFrom(targetFile);
                return false;
            }
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
                // Cancel download if etag header matches the locally stored one:
                if (self.HasMatchingChecksum(headers.GetEtagHeader())) { return true; }
                // Cancel download if local file with the same MD5 hash exists:
                var onlineMD5 = headers.GetMD5Checksum();
                if (!onlineMD5.IsNullOrEmpty()) {
                    if (self.HasMatchingChecksum(onlineMD5)) { return true; }
                    if (onlineMD5 == targetFile.CalcFileMd5Hash()) { return true; }
                    return false;
                }
                /*
                // Cancel download if local file with the exact last-write timestamp exists:
                if (headers.GetRawLastModifiedString() != null) {
                    var distance = headers.GetLastModifiedUtcDate(DateTime.MinValue) - targetFile.LastWriteTime.ToUniversalTime();
                    Log.d("distance.Milliseconds: " + distance.Milliseconds);
                    if (distance.Milliseconds == 0) { return true; }
                }
                */
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
            SetLocalFileInfosFrom(self, targetFile);
            self.mimeType = headers.GetContentMimeType(self.mimeType);
            if (headers.GetRawLastModifiedString() != null) {
                targetFile.LastWriteTime = headers.GetLastModifiedUtcDate(DateTime.MinValue);
            }
            if (headers.GetEtagHeader() != null) {
                self.AddCheckSum(CHECKSUM_ETAG, headers.GetEtagHeader());
            }
        }

        private static void SetLocalFileInfosFrom(this IFileRef self, FileEntry targetFile) {
            self.AssertValidDirectory(targetFile.Parent);
            self.dir = targetFile.Parent.FullName;
            self.fileName = targetFile.Name;
        }

        private static void AssertValidDirectory(this IFileRef self, DirectoryEntry targetDirectory) {
            if (!targetDirectory.IsNotNullAndExists()) {
                throw new ArgumentException("Cant download into non existing directory=" + targetDirectory);
            }
            if (self.dir != null && targetDirectory.FullName != self.dir) {
                throw new ArgumentException($"Dir already set, wont change from '{self.dir}' to '{targetDirectory}'");
            }
        }

        private static async Task<string> CalculateFileName(IFileRef self, RestRequest request) {
            if (!self.fileName.IsNullOrEmpty()) { return self.fileName; }
            try {
                Headers headers = await request.GetResultHeaders();
                var nameOnServer = headers.GetFileNameOnServer();
                if (!nameOnServer.IsNullOrEmpty()) { return nameOnServer; }
                var hashedHeaders = headers.GenerateHashNameFromHeaders();
                if (!hashedHeaders.IsNullOrEmpty()) { return hashedHeaders; }
            } catch (Exception e) {
                Log.w("Error while accessing remote request headers:", e);
            }
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
