using System;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.progress;

namespace com.csutil {

    public class CountdownTimer {

        public static CountdownTimer StartNew(int timerDurationInMs, Action<Task> onComplete = null) {
            var timer = new CountdownTimer();
            timer.StartTimer(timerDurationInMs);
            if (onComplete != null) { timer.timerTask.ContinueWithSameContext(onComplete); }
            return timer;
        }

        public StopwatchV2 timer { get; private set; }
        public Task timerTask { get; private set; }
        public int timerDurationInMs { get; private set; }
        public long ElapsedMilliseconds => timer.ElapsedMilliseconds > timerDurationInMs ? timerDurationInMs : timer.ElapsedMilliseconds;
        
        private CancellationTokenSource cancellationToken;

        public Task StartTimer(int timerDurationInMs) {
            timerTask = NewTimerTask(timerDurationInMs);
            return timerTask;
        }

        public void CancelTimer() {
            cancellationToken.Cancel();
            Stop();
        }

        public void AddProgressListener(IProgress targetProgressToUpdate, int updateIntervalInMs) {
            MonitorProgress(targetProgressToUpdate, updateIntervalInMs);
        }
        
        private async Task NewTimerTask(int timerDurationInMs) {
            this.timerDurationInMs = timerDurationInMs;
            cancellationToken = new CancellationTokenSource();
            timer = StopwatchV2.StartNewV2();
            await TaskV2.Delay(timerDurationInMs, cancellationToken.Token);
            int durationSoFar = (int)timer.ElapsedMilliseconds;
            if (durationSoFar < timerDurationInMs) {
                await TaskV2.Delay(timerDurationInMs - durationSoFar);
            }
            Stop();
        }

        private void Stop() { timer.StopV2(); }

        private async Task MonitorProgress(IProgress targetProgressToUpdate, int updateIntervalInMs) {
            do {
                targetProgressToUpdate.SetCount(ElapsedMilliseconds, timerDurationInMs);
                await TaskV2.Delay(updateIntervalInMs);
            } while (!timerTask.IsCompleted);
            targetProgressToUpdate.SetComplete();
        }

    }

}