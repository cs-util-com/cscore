using System.IO;
using Xunit;
using ZstdSharp;

namespace com.csutil.tests {

    public class ZstdCompressionTests {

        public ZstdCompressionTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            var dir = EnvironmentV2.instance.GetOrAddTempFolder("ZstdCompressionTests");
            var inputFile = dir.GetChild("SomeFile1.txt");
            var zippedFile = dir.GetChild("SomeFile1.txt.zst");
            inputFile.WriteAllText("Some text to compress");

            { // Streaming compression:
                using var inputStream = inputFile.OpenForRead();
                using var targetStreamToWriteTo = zippedFile.OpenOrCreateForWrite();
                using var compressionStream = new CompressionStream(targetStreamToWriteTo, Compressor.MaxCompressionLevel);
                inputStream.CopyTo(compressionStream);
            }
            { // Streaming decompression:
                using var input = zippedFile.OpenForRead();
                using var output = new MemoryStream();
                using var decompressionStream = new DecompressionStream(input);
                decompressionStream.CopyTo(output);
                output.ResetStreamCurserPositionToBeginning();
                var decompressedText = new StreamReader(output).ReadToEnd();
                Assert.Equal(inputFile.ReadAllText(), decompressedText);
            }
        }

    }

}