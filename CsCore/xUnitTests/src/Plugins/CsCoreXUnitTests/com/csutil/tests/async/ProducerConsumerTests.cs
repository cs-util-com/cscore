using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.async {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class ProducerConsumerTests {

        public ProducerConsumerTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        /// <summary> See https://learn.microsoft.com/en-us/dotnet/core/extensions/channels </summary>
        [Fact]
        public async Task ExampleUsageOfChannels() {

            var producerConsumerChannel = Channel.CreateUnbounded<MyClass1>();
            var count = 100000;

            var readTask = ReadAllEventsFromChannel(producerConsumerChannel, count);

            for (int i = 1; i <= count; i++) {
                ValueTask _ = producerConsumerChannel.Writer.WriteAsync(new MyClass1() { Id = i });
            }
            // No further events will be written (allows the async channel reader to complete): 
            producerConsumerChannel.Writer.Complete();

            // The read task will be able to complete now successfully:
            await readTask;


        }

        private static async Task ReadAllEventsFromChannel(Channel<MyClass1> producerConsumerChannel, int count) { // Start reading all events from the channel: 
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