﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace com.csutil.progress {

    public class ProgressManager {

        public static void SetupSingleton() {
            var pm = new ProgressManager();
            IoC.inject.SetSingleton(pm);
            // Enable that pm reacts to injection requests for IProgress:
            IoC.inject.RegisterInjector(pm, pm.ProgressInjectionRequest);
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

        public IProgress ProgressInjectionRequest(object caller, bool createIfNull) {
            if (caller is null) {
                Log.e("ProgressInjectionRequest was called with null caller: " + caller);
                return null;
            }
            if (caller is string id) { return GetOrAddProgress(id, 0, createIfNull); }
            if (caller is KeyValuePair<string, double> p) { return GetOrAddProgress(p.Key, p.Value, createIfNull); }
            throw new ArgumentException($"Cant handle caller='{caller}'");
        }

        public void RemoveProcesses(IEnumerable<KeyValuePair<string, IProgress>> processes) {
            foreach (var p in processes) { trackedProgress.Remove(p); }
            OnProgressChanged(null);
        }

        public IEnumerable<KeyValuePair<string, IProgress>> GetCompletedProgresses() {
            return trackedProgress.Filter(x => x.Value.IsComplete()).ToList();
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
            combinedAvgPercent = totalTasks > 0 ? percentSum / totalTasks : 0;
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
            if (progressThatJustChanged != null) { latestUpdatedProgress = progressThatJustChanged; }
            CalcLatestStats();
            OnProgressUpdate?.Invoke(this, progressThatJustChanged);
        }

    }

}