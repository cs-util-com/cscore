using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.integrationTests.model.immutable {

    public class ImmutableCollectionTests {

        public ImmutableCollectionTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => {
                for (int i = 0; i < 1000; i++) {
                    await RunTestOnSet(new HashSet<string>());
                }
            });
            {
                for (int i = 0; i < 1000; i++) {
                    await RunTestOnSet(new ConcurrentSet<string>());
                }
            }
        }

        private static async Task RunTestOnSet(ISet<string> set) {
            var count = 100;
            var tasks = new List<Task<bool>>();
            for (int j = 0; j < count; j++) {
                tasks.Add(TaskV2.Run(async () => set.Add("abc")));
            }
            await Task.WhenAll(tasks);
            Assert.Equal(count, tasks.Count);
            var successCount = tasks.Filter(x => x.Result).Count();
            if (1 != successCount) {
                throw new InvalidOperationException("Expected only one task to return true but instead it was " + successCount);
            }
        }

    }

}