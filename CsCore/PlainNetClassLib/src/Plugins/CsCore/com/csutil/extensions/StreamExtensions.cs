
using System.IO;

namespace com.csutil {

    public static class StreamExtensions {

        public static byte[] ToByteArray(this Stream self) {
            if (self is MemoryStream memStream) { return memStream.ToArray(); }
            using (MemoryStream memoryStream = new MemoryStream()) {
                self.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

    }

}