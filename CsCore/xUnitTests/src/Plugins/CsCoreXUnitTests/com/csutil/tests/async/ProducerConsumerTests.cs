﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

#if !UNITY_5_3_OR_NEWER

using System.Threading.Channels;

namespace com.csutil.tests.async {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class ProducerConsumerTests {

        public ProducerConsumerTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        /// <summary> See https://learn.microsoft.com/en-us/dotnet/core/extensions/channels
        /// and https://learn.microsoft.com/en-us/shows/on-net/working-with-channels-in-net </summary>
        [Fact]
        public async Task ExampleUsageOfChannels() {
            var count = 10000;
            {
                var producerConsumerChannel = Channel.CreateUnbounded<MyClass1>();
                await WriteAllEventsToChannel(producerConsumerChannel, count);
                await ReadAllEventsFromChannel(producerConsumerChannel, count);
            }
            {
                var producerConsumerChannel = Channel.CreateUnbounded<MyClass1>();
                var readTask = ReadAllEventsFromChannel(producerConsumerChannel, count);
                var writeTask = WriteAllEventsToChannel(producerConsumerChannel, count);
                await writeTask;
                await readTask;
            }
            { // Only allow a single item to be in the channel at once:
                var producerConsumerChannel = Channel.CreateBounded<MyClass1>(1);
                var readTask = ReadAllEventsFromChannel(producerConsumerChannel, count);
                var writeTask = WriteAllEventsToChannel(producerConsumerChannel, count);
                await writeTask; // Since the continuous reader is already set up, write can be awaited here
                await readTask;
            }
        }

        private static async Task WriteAllEventsToChannel(Channel<MyClass1> producerConsumerChannel, int count) {
            // Writing should be awaited in case only a limited number of elements is allowed to be added to the channel at once:
            for (int i = 1; i <= count; i++) {
                await producerConsumerChannel.Writer.WriteAsync(new MyClass1() { Id = i });
            }
            // No further events will be written (allows the async channel reader to complete): 
            producerConsumerChannel.Writer.Complete();
        }

        private static async Task ReadAllEventsFromChannel(Channel<MyClass1> producerConsumerChannel, int count) {
            MyClass1 y1 = null;
            while (await producerConsumerChannel.Reader.WaitToReadAsync()) {
                while (producerConsumerChannel.Reader.TryRead(out var y2)) {
                    if (y1 != null) {
                        // Check that the items returned by the reader are in order:
                        Assert.Equal(y1.Id + 1, y2.Id);
                    }
                    y1 = y2;
                }
            }
            Assert.NotNull(y1);
            // The reader received all items:
            Assert.Equal(count, y1.Id);
        }

        public class MyClass1 {
            public int Id { get; set; }
        }

    }

}

#endif