using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.model.immutable {

    public class ImmutableCollectionTests {

        public ImmutableCollectionTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            await Assert.ThrowsAsync<Xunit.Sdk.EqualException>(async () => {
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
            var tasks = new List<Task<bool>>();
            for (int j = 0; j < 100; j++) {
                tasks.Add(TaskV2.Run(async () => set.Add("abc")));
            }
            await Task.WhenAll(tasks);
            Assert.Equal(1, tasks.Filter(x => x.Result).Count());
        }

    }

}