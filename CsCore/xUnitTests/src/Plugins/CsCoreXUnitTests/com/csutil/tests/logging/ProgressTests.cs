
using System;
using System.Threading.Tasks;
using com.csutil.progress;
using Xunit;

namespace com.csutil.tests {

    public class ProgressTests {

        public ProgressTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {
            using (IProgress progress = new ProgressV2(id: "Download Progress 1", totalCount: 10)) {
                Assert.Equal(10, progress.totalCount);
                progress.SetCount(7);
                Assert.Equal(70, progress.percent);
                progress.IncrementCount();
                Assert.Equal(80, progress.percent);
                progress.percent = 90;
                Assert.Equal(9, progress.GetCount());
                progress.SetComplete();
                Assert.True(progress.IsComplete());
                Assert.Equal(100, progress.percent);
            }
        }

        [Fact]
        public void TestIProgressReport() {
            using (ProgressV2 progress = new ProgressV2("p2", 100)) {
                // Can be used as a System.IProgress:
                IProgress<double> sysProgr = progress;
                sysProgr.Report(99);
                Assert.Equal(100, progress.totalCount);
                Assert.Equal(99, progress.percent);
                Assert.Equal(99, progress.GetCount());
            }
        }

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
                await TaskV2.Delay(50); // Wait for progress update to be invoked
                Assert.True(progressEventTriggered);
            }
        }

        [Fact]
        public void TestDispose() {
            ProgressV2 progress = new ProgressV2("testDispose", 200);
            progress.percent = 50;
            Assert.Equal(100, progress.GetCount());
            progress.totalCount = 400;
            Assert.Equal(100, progress.GetCount());
            Assert.Equal(25, progress.percent);

            progress.Dispose();
            Assert.Throws<ObjectDisposedException>(() => progress.totalCount = 100);
            Assert.Throws<ObjectDisposedException>(() => progress.percent = 1);
            Assert.Throws<ObjectDisposedException>(() => progress.SetCount(2));
            Assert.Throws<ObjectDisposedException>(() => ((IProgress<double>)progress).Report(3));
        }

    }

}