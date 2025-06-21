using System;
using System.IO;
using System.Threading;
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

        public static T ResetStreamCurserPositionToBeginning<T>(this T self) where T : Stream {
            if (!self.CanSeek) { throw new InvalidOperationException("Stream not seekable, cant jump back to start of stream, first do stream.CopyToSeekableStreamIfNeeded() ?"); }
            self.Position = 0; // Move curser back to beginning after copy
            return self;
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

        public static async Task<Stream> CopyToSeekableStreamIfNeeded(this Task<Stream> stream, bool disposeOriginalStream) { 
            return await (await stream).CopyToSeekableStreamIfNeeded(disposeOriginalStream);
        }

        public static async Task<Stream> CopyToSeekableStreamIfNeeded(this Stream stream, bool disposeOriginalStream) {
            if (stream.CanSeek) { return stream; }
            var seekableStream = new MemoryStream();
            await stream.CopyToAsync(seekableStream);
            if (disposeOriginalStream) { stream.Dispose(); }
            seekableStream.Seek(0, SeekOrigin.Begin);
            return seekableStream;
        }

        public static async Task MonitorPositionForProgress(this Stream self, Action<long> onProgress, CancellationTokenSource cancel, int delayInMsBetweenProgress = 10) {
            onProgress.ThrowErrorIfNull("onProgress");
            long progress = 0;
            do {
                cancel?.Token.ThrowIfCancellationRequested();
                await TaskV2.Delay(delayInMsBetweenProgress);
                long newProgress = 100L * self.Position / self.Length;
                if (progress != newProgress) { onProgress(newProgress); }
                progress = newProgress;
            } while (progress < 100);
        }

    }

}