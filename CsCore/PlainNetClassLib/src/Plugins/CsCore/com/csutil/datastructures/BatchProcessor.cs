using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.model;

namespace com.csutil {

    public abstract class BatchProcessor<E> : KeyValueStoreTypeAdapter<E>, IDisposable where E : HasId {

        public int batchSize { get; set; }
        private int batchingInProgress = 0;
        private readonly CancellationTokenSource cancel;

        public BatchProcessor(IKeyValueStore localCache, int batchSize, CancellationTokenSource cancel) : base(localCache) {
            this.batchSize = batchSize;
            this.cancel = cancel;
        }

        public Task<E> Add(E val) {
            return Set(val.GetId(), val);
        }

        public override async Task<E> Set(string key, E val) {
            cancel.Token.ThrowIfCancellationRequested();
            var e = await base.Set(key, val);
            BatchProcessIfNeeded().LogOnError();
            return e;
        }

        public virtual async Task BatchProcessIfNeeded() {
            cancel.Token.ThrowIfCancellationRequested();
            if (IsNextBatchReadyForProcessing(await store.GetAllKeys())) {
                await BatchProcess();
                // If batch processing was needed run it again in case it is still needed:
                await BatchProcessIfNeeded();
            } else {
                OnBatchWasNotYetReadyForProcessing();
            }
        }

        protected virtual void OnBatchWasNotYetReadyForProcessing() { }

        protected virtual bool IsNextBatchReadyForProcessing(IEnumerable<string> keys) {
            return keys.CountIsAbove(batchSize - 1);
        }

        public async Task BatchProcess() {
            cancel.Token.ThrowIfCancellationRequested();
            if (ThreadSafety.FlipToTrue(ref batchingInProgress)) {
                try {
                    var d = default(E);
                    cancel.Token.ThrowIfCancellationRequested();
                    var keys = await store.GetAllKeys();
                    if (!keys.IsEmpty()) {
                        cancel.Token.ThrowIfCancellationRequested();
                        var entriesToProcess = await keys.MapAsync(k => store.Get<E>(k, d));
                        cancel.Token.ThrowIfCancellationRequested();
                        var processedEntries = await Process(entriesToProcess, cancel.Token);
                        foreach (var entryToDelete in processedEntries) {
                            if (!await store.Remove(entryToDelete.GetId())) {
                                await OnCouldNotRemoveProcessedEntry(entryToDelete);
                            }
                        }
                    }
                } finally {
                    ThreadSafety.FlipToFalse(ref batchingInProgress);
                }
            }
        }

        protected virtual Task OnCouldNotRemoveProcessedEntry(E entryToDelete) {
            Log.w($"Could not remove Entry {entryToDelete.GetId()} {entryToDelete}");
            return Task.CompletedTask;
        }

        /// <summary> Will be called once a batch of entries is ready to be batch processed </summary>
        /// <param name="entriesToProcess">All entries that are waiting to be batch processed </param>
        /// <param name="cancellationToken"> The cancel token that has to be checked if the batch process is canceled </param>
        /// <returns> The keys that where batch processed and can be removed from the local cache as the next automatic step </returns>
        protected abstract Task<IEnumerable<E>> Process(IEnumerable<E> entriesToProcess, CancellationToken cancellationToken);

        public void Dispose() {
            store.Dispose();
            if (!cancel.IsCancellationRequested) { cancel.Cancel(); }
        }

    }

}