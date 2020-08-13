using System;
using System.Collections.Generic;

namespace com.csutil.progress {

    public class ProgressManager {

        public static void SetupSingleton() {
            var pm = new ProgressManager();
            IoC.inject.SetSingleton(pm);
            pm.RegisterProgressInjector();
        }

        public event EventHandler<IProgress> OnProgressUpdate;
        private IDictionary<string, IProgress> trackedProgress = new Dictionary<string, IProgress>();

        public DateTime latestChangeTime { get; private set; }
        public IProgress latestUpdatedProgress { get; private set; }

        public double combinedCount { get; private set; }
        public double combinedTotalCount { get; private set; }
        public double combinedPercent { get; private set; }
        public double combinedAvgPercent { get; private set; }

        public int totalTasks { get; private set; }
        public int finishedTasks { get; private set; }

        private void RegisterProgressInjector() { IoC.inject.RegisterInjector(this, ProgressInjectionRequest); }

        private IProgress ProgressInjectionRequest(object caller, bool createIfNull) {
            AssertV2.IsNotNull(caller, "caller");
            if (caller is string id) { return GetOrAddProgress(id, 0, createIfNull); }
            if (caller is KeyValuePair<string, double> p) { return GetOrAddProgress(p.Key, p.Value, createIfNull); }
            throw new ArgumentException($"Cant handle caller='{caller}'");
        }

        public void CalcLatestStats() {
            double percentSum = 0;
            double countSum = 0;
            double totalCountSum = 0;
            int finishedTasksCounter = 0;
            foreach (var p in trackedProgress.Values) {
                percentSum += p.GetCappedPercent();
                countSum += p.GetCount();
                totalCountSum += p.totalCount;
                if (p.percent >= 100) { finishedTasksCounter++; }
            }
            totalTasks = trackedProgress.Count;
            combinedAvgPercent = percentSum / totalTasks;
            combinedCount = countSum;
            combinedTotalCount = totalCountSum;
            combinedPercent = 100d * combinedCount / combinedTotalCount;
            finishedTasks = finishedTasksCounter;
        }

        public IProgress GetOrAddProgress(string id, double totalCount, bool createIfNull) {
            id.ThrowErrorIfNullOrEmpty("id");
            if (trackedProgress.TryGetValue(id, out IProgress existingProgress)) { return existingProgress; }
            if (!createIfNull) { throw new KeyNotFoundException($"Porgess '{id}' NOT found and createIfNull is false"); }
            var newProgress = new ProgressV2(id, totalCount);
            trackedProgress.Add(id, newProgress);
            newProgress.ProgressChanged += (o, e) => { OnProgressChanged(newProgress); };
            OnProgressChanged(newProgress);
            return newProgress;
        }

        public IProgress GetProgress(string id) {
            id.ThrowErrorIfNullOrEmpty("id");
            if (trackedProgress.TryGetValue(id, out IProgress p)) { return p; }
            throw new KeyNotFoundException("No Progress found with id " + id);
        }

        private void OnProgressChanged(IProgress progressThatJustChanged) {
            latestChangeTime = DateTimeV2.UtcNow;
            latestUpdatedProgress = progressThatJustChanged;
            OnProgressUpdate?.Invoke(this, progressThatJustChanged);
        }

    }

}