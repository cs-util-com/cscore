using NUnit.Framework;
using System.IO;
using System.Text;

namespace com.csutil.tests.io {

    public class ResourcesV2Tests {

        [Test]
        public void TestLoadingBinaryDataFromResources() {
            var stream = ResourcesV2.LoadV2<Stream>("SomeBinaryData") as MemoryStream;
            Assert.NotNull(stream);
            Assert.NotZero(stream.Length);
            var textFromBinaryFile = Encoding.UTF8.GetString(stream.ToArray());
            Assert.AreEqual("Some data", textFromBinaryFile);
        }

    }

}
