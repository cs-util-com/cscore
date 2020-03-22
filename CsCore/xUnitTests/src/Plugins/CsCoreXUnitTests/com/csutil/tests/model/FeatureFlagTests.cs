using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.json;
using com.csutil.keyvaluestore;
using com.csutil.model;
using com.csutil.tests.keyvaluestore;
using Xunit;

namespace com.csutil.tests.model {

    public class FeatureFlagTests {

        public FeatureFlagTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            AssertV2.throwExeptionIfAssertionFails = true;

            // Setup a feature flag manager that is connected to a remote server and has a local cache when offline:
            var simulatedOnlineStore = new SimulatedOnlineFeatureFlagStore();
            var simulatedInetConnection = new MockDelayKeyValueStore(simulatedOnlineStore);
            simulatedInetConnection.delay = 10;
            var localCache = new InMemoryKeyValueStore().WithFallbackStore(new ExceptionWrapperKeyValueStore(simulatedInetConnection));
            IoC.inject.SetSingleton(new FeatureFlagManager(localCache));

            // Feature flag not 
            Assert.False(await FeatureFlag.IsEnabled("MyFlag1"));

            simulatedOnlineStore.flagToReturn = new FeatureFlag() { isEnabled = false };
            Assert.False(await FeatureFlag.IsEnabled("MyFlag1"));
            simulatedOnlineStore.flagToReturn.isEnabled = true;

            // The local store will first time return its cached value and fetch the new online one asynchronously:
            Assert.False(await FeatureFlag.IsEnabled("MyFlag1")); // Trigger request to server, still old state returned
            await Task.Delay(40); // Wait for server request to finish, so that local state is updated
            Assert.True(await FeatureFlag.IsEnabled("MyFlag1"));

            simulatedOnlineStore.flagToReturn.isStagedRollout = true;
            simulatedOnlineStore.flagToReturn.rolloutPercentServer = 0;

            Assert.Equal(0, simulatedOnlineStore.flagToReturn.localRandomValue);

            var f = new FeatureFlag();
            Assert.IsType<FeatureFlag>(f);
            Assert.IsType<FeatureFlag>(f.DeepCopyViaJson());


            Assert.True(await FeatureFlag.IsEnabled("MyFlag1"));  // Trigger request to server, still old state returned
            await Task.Delay(40); // Wait for server request to finish, so that local state is updated
            Assert.False(await FeatureFlag.IsEnabled("MyFlag1"));

            Assert.True(simulatedOnlineStore.flagToReturn.localRandomValue > 0);
            Assert.True(simulatedOnlineStore.flagToReturn.localRandomValue < 100);

            simulatedOnlineStore.flagToReturn.rolloutPercentServer = simulatedOnlineStore.flagToReturn.localRandomValue + 1;
            Assert.False(await FeatureFlag.IsEnabled("MyFlag1"));  // Trigger request to server, still old state returned
            await Task.Delay(40); // Wait for server request to finish, so that local state is updated
            Assert.True(await FeatureFlag.IsEnabled("MyFlag1"));

            // User not online anymore
            simulatedInetConnection.throwTimeoutError = true;
            // Make sure the local cache of the flags still returns the flag:
            Assert.True(await FeatureFlag.IsEnabled("MyFlag1"));

            simulatedInetConnection.throwTimeoutError = false; // internet is back
            simulatedOnlineStore.flagToReturn = null; // the server disabled the feature flag again

            Assert.True(await FeatureFlag.IsEnabled("MyFlag1")); // Trigger request to server, still old state returned
            await Task.Delay(40); // Wait for server request to finish, so that local state is updated
            Assert.False(await FeatureFlag.IsEnabled("MyFlag1"));

        }

        private class SimulatedOnlineFeatureFlagStore : IKeyValueStore {

            public FeatureFlag flagToReturn; // This simulates the flag stored in the remote server 

            public Task<T> Get<T>(string key, T defaultValue) {

                var featureKey = TypedJsonHelper.NewTypedJsonReader().Read<FeatureFlagManager.FeatureKey>(key);
                Assert.Equal("MyFlag1", featureKey.id);
                Assert.Equal(typeof(FeatureFlag), typeof(T));
                if (flagToReturn == null) { return Task.FromResult((T)(object)new FeatureFlag() { isEnabled = false }); }
                return Task.FromResult((T)(object)flagToReturn);
            }

            public Task<object> Set(string key, object value) {
                var featureKey = TypedJsonHelper.NewTypedJsonReader().Read<FeatureFlagManager.FeatureKey>(key);
                Assert.Equal("MyFlag1", featureKey.id);
                Assert.NotNull(value);
                Assert.IsType<FeatureFlag>(value);
                var f = value as FeatureFlag;
                Assert.NotEqual(0, f.localRandomValue);
                Assert.NotEqual(f, flagToReturn);
                flagToReturn = f;
                return Task.FromResult(value);
            }

            public IKeyValueStore fallbackStore { get; set; }
            public Task<bool> ContainsKey(string key) { throw new NotImplementedException(); }
            public void Dispose() { throw new NotImplementedException(); }
            public Task<IEnumerable<string>> GetAllKeys() { throw new NotImplementedException(); }
            public Task<bool> Remove(string key) { throw new NotImplementedException(); }
            public Task RemoveAll() { throw new NotImplementedException(); }

        }

    }

}