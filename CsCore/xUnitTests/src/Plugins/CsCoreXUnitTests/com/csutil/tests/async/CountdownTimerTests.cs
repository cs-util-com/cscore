using System.Threading.Tasks;
using com.csutil.progress;
using Xunit;

namespace com.csutil.tests.async {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class CountdownTimerTests {

        public CountdownTimerTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            var timerDurationInMs = 300;

            bool completedCallbackReceived = false;
            var timer = CountdownTimer.StartNew(timerDurationInMs, onComplete: delegate {
                completedCallbackReceived = true;
            });

            await TaskV2.Delay(10);
            Assert.True(timer.ElapsedMilliseconds >= 10, "timer.ElapsedMilliseconds=" + timer.ElapsedMilliseconds);

            await timer.timerTask; // Wait for the end of the timer

            var duration = timer.ElapsedMilliseconds;
            await TaskV2.Delay(10);
            Assert.Equal(duration, timer.ElapsedMilliseconds); // The timer stopped
            Assert.True(completedCallbackReceived);
            Assert.True(duration >= timerDurationInMs, "duration=" + duration);
        }

        [Fact]
        public async Task ExampleUsage2() {
            var timer = CountdownTimer.StartNew(100);
            await TaskV2.Delay(10);

            IProgress p = new ProgressV2("TimerProgress", 1);
            timer.AddProgressListener(p, updateIntervalInMs: 5);
            Assert.True(10 < p.percent); // 10% of the countdown must already have passed
            await timer.timerTask; // Wait for the end of the timer
            await TaskV2.Delay(40); // Delay to ensure at least one more update to p happened
            Assert.Equal(100, p.percent);
            Assert.True(p.IsComplete());
        }

        [Fact]
        public async Task CancelTest() {
            int timerDurationInMs = 100;
            var timer = CountdownTimer.StartNew(timerDurationInMs);
            await TaskV2.Delay(10);
            timer.CancelTimer();
            await Assert.ThrowsAsync<TaskCanceledException>(async () => {
                await timer.timerTask;
            });
            var duration = timer.ElapsedMilliseconds;
            Assert.True(timerDurationInMs > timer.ElapsedMilliseconds);
            await TaskV2.Delay(10); // Ensure the timer stopped
            Assert.Equal(duration, timer.ElapsedMilliseconds);
        }

    }

}