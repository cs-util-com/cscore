using System;
using System.Threading.Tasks;
using com.csutil.progress;
using Xunit;

namespace com.csutil.integrationTests {
    
    [Collection("Sequential")] // Will execute tests in here sequentially
    public class ProgressIntegrationTests {

        public ProgressIntegrationTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task TestEventListeners() {
            using (ProgressV2 progress = new ProgressV2("p3", 200)) {
                var progressEventTriggered = false;
                progress.ProgressChanged += (o, newValue) => {
                    Log.e(JsonWriter.AsPrettyString(o));
                    Assert.Equal(100, newValue);
                    progressEventTriggered = true;
                };
                await TaskV2.Run(() => {
                    ((IProgress<double>)progress).Report(100);
                    Assert.False(progressEventTriggered);
                    Assert.Equal(100, progress.GetCount());
                    Assert.Equal(50, progress.percent);
                });
                Assert.Equal(100, progress.GetCount());
                Assert.Equal(50, progress.percent);

                // The progress callback is dispatched using the SynchronizationContext 
                // of the constructor, so it will have some delay before being called:
                Assert.False(progressEventTriggered);
                await TaskV2.Delay(200); // Wait for progress update to be invoked
                Assert.True(progressEventTriggered);
            }
        }

    }
    
}