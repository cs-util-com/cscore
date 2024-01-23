using com.csutil.src.Plugins.CsCore.com.csutil.algorithms;
using Xunit;

namespace com.csutil.tests.AlgorithmTests {
    public class ImageCompareTests {

        public ImageCompareTests(Xunit.Abstractions.ITestOutputHelper logger) { 
        
            logger.UseAsLoggingOutput();

        }

        [Fact]
        public void TestImageCompare()
        {

            AaaaImageCompareAaaa compareObj = new AaaaImageCompareAaaa();
            Assert.Equal(100, compareObj.ImageCompare());

        }

    }

}
