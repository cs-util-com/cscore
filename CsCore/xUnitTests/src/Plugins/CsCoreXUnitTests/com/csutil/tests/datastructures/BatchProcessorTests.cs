using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.model;
using Xunit;

namespace com.csutil.tests.keyvaluestore {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class BatchProcessorTests {

        public BatchProcessorTests(Xunit.Abstractions.ITestOutputHelper logger) {
            logger.UseAsLoggingOutput();
        }

        private class MyEvent : HasId {
            private string id = GuidV2.NewGuid().ToString();
            public string Name;
            public bool WasProcessed { get; set; }
            public string GetId() { return id; }
        }

        private class MyBatchProcessor : BatchProcessor<MyEvent> {

            public int ProcessCalledCounter = 0;

            public MyBatchProcessor(IKeyValueStore localCache, int batchSize, CancellationTokenSource cancel) : base(localCache, batchSize, cancel) { }

            protected override async Task<IEnumerable<MyEvent>> Process(IEnumerable<MyEvent> entriesToProcess, CancellationToken cancellationToken) {
                Log.MethodEntered("entriesToProcess=" + entriesToProcess.Count());
                ProcessCalledCounter++;
                foreach (var entry in entriesToProcess) { entry.WasProcessed = true; }
                return entriesToProcess;
            }

            protected override void OnBatchWasNotYetReadyForProcessing() { DelayAndThenForceProcessBatch().LogOnError(); }

            private async Task DelayAndThenForceProcessBatch() {
                Log.MethodEntered();
                await TaskV2.Delay(2000);
                await BatchProcess();
            }

        }

        [Fact]
        public async Task ExampleUsage1() {
            var batchSize = 5;
            var processor = new MyBatchProcessor(new InMemoryKeyValueStore(), batchSize, new CancellationTokenSource());

            for (int i = 1; i < batchSize; i++) {
                await processor.Add(new MyEvent() { Name = "" + i });
                var keys = await processor.GetAllKeys();
                await Task.Delay(20);
                Assert.Equal(i, keys.Count());
            }
            // No event was processed yet
            Assert.Empty((await processor.GetAll()).Filter(e => e.WasProcessed));

            // Add the final element to reach the batch size:
            await processor.Add(new MyEvent() { Name = "" + batchSize });
            await Task.Delay(20);
            Assert.Empty(await processor.GetAllKeys());

            // All events are processed now:
            Assert.Empty((await processor.GetAll()).Filter(e => !e.WasProcessed));
            Assert.Equal(1, processor.ProcessCalledCounter);

            // Adding another entry will trigger the OnBatchWasNotYetReadyForProcessing and force process it 
            await processor.Add(new MyEvent() { Name = "" + (batchSize + 1) });
            await TaskV2.Delay(2100);
            Assert.Equal(2, processor.ProcessCalledCounter);

            Assert.Empty(await processor.GetAllKeys());
            Assert.Empty((await processor.GetAll()).Filter(e => !e.WasProcessed));

        }

    }

}