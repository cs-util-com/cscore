
using System;
using System.IO;
using System.Threading.Tasks;

namespace com.csutil {

    public static class StreamExtensions {

        public static byte[] ToByteArray(this Stream self) {
            if (self is MemoryStream memStream) { return memStream.ToArray(); }
            using (MemoryStream memoryStream = new MemoryStream()) {
                self.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static void CopyTo(this Stream self, Stream destination, Action<long> onProgress, int bufferSize = 4096) {
            byte[] buffer = new byte[bufferSize];
            int numBytes;
            long bytesCopied = 0;
            while ((numBytes = self.Read(buffer, 0, buffer.Length)) > 0) {
                bytesCopied += numBytes;
                onProgress(bytesCopied);
                destination.Write(buffer, 0, numBytes);
            }
        }

        public static async Task CopyToAsync(this Stream self, Stream destination, Action<long> onProgress, int bufferSize = 4096) {
            byte[] buffer = new byte[bufferSize];
            int numBytes;
            long bytesCopied = 0;
            while ((numBytes = await self.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                bytesCopied += numBytes;
                onProgress(bytesCopied);
                await destination.WriteAsync(buffer, 0, numBytes);
            }
        }

        public static async Task<Stream> CopyToSeekableStreamIfNeeded(this Stream stream, bool disposeOriginalStream) {
            if (stream.CanSeek) { return stream; }
            var seekableStream = new MemoryStream();
            await stream.CopyToAsync(seekableStream);
            if (disposeOriginalStream) { stream.Dispose(); }
            seekableStream.Seek(0, SeekOrigin.Begin);
            return seekableStream;
        }

    }

}