using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.io;
using com.csutil.model;
using StbImageSharp;
using Zio;

namespace com.csutil.tests.AlgorithmTests.images {

    /// <summary> Used for testing the different image algorithms </summary>
    public class MyImageFileRef : IFileRef {

        public string dir { get; set; }
        public string fileName { get; set; }
        public string url { get; set; }
        public Dictionary<string, object> checksums { get; set; }
        public string mimeType { get; set; }

        public static async Task<ImageResult> DownloadFileIfNeeded(DirectoryEntry folder, string url) {
            var imgFileRef = new MyImageFileRef() { url = url };
            await imgFileRef.DownloadTo(folder, useAutoCachedFileRef: true);
            var imgFile = imgFileRef.GetFileEntry(folder.FileSystem);
            return await ImageLoader.LoadImageInBackground(imgFile);
        }

        public static async Task<ImageResult> DownloadFileIfNeeded(FileEntry self, string url) {
            var imgFileRef = new MyImageFileRef() { url = url, fileName = self.Name };
            await imgFileRef.DownloadTo(self.Parent, useAutoCachedFileRef: true);
            return await ImageLoader.LoadImageInBackground(self);
        }

    }

}