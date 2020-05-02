using com.csutil.keyvaluestore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests {

    public class PreferencesTests {

        public PreferencesTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]

        public void ExampleUsage1() {

            // In the setup logic of your application set the Preferences singleton:
            IoC.inject.SetSingleton<IPreferences>(new Preferences(new InMemoryKeyValueStore()));

            string key1 = "key1";
            string value1 = "value1";
            var prefs = Preferences.instance;
            prefs.Set(key1, value1);
            string x = prefs.Get(key1, "defaultValue1").Result;
            prefs.Remove(key1); // cleanup
            Assert.Equal(value1, x);

        }

        [Fact]
        public async Task ExampleUsage2() {

            var store = new InMemoryKeyValueStore();
            var prefs = new Preferences(store);
            IoC.inject.SetSingleton<IPreferences>(prefs);

            var firstStart = prefs.GetFirstStartDate();
            var lastUpdate = prefs.GetLastUpdateDate();
            Assert.True(firstStart > 0, "firstStart=" + firstStart);
            Assert.True(lastUpdate > 0, "lastUpdate=" + lastUpdate);
            var diffInMs = Math.Abs(firstStart - lastUpdate);
            Assert.True(diffInMs < 1000, "diffInMs=" + diffInMs);

            prefs = new Preferences(store);
            Assert.Equal(firstStart, prefs.GetFirstStartDate());
            Assert.Equal(lastUpdate, prefs.GetLastUpdateDate());

            var sysInfo = EnvironmentV2.instance.systemInfo as EnvironmentV2.SystemInfo;

            Assert.True(DateTime.UtcNow.ToUnixTimestampUtc() > 0, "DateTime.UtcNow=" + DateTime.UtcNow.ToUnixTimestampUtc());
            Assert.True(DateTimeV2.UtcNow.ToUnixTimestampUtc() > 0, "DateTimeV2.UtcNow=" + DateTimeV2.UtcNow.ToUnixTimestampUtc());
            Assert.True(sysInfo.latestLaunchDate > 0, "sysInfo.latestLaunchDate=" + sysInfo.latestLaunchDate);
            Assert.NotEqual(0, sysInfo.latestLaunchDate);
            Assert.NotNull(sysInfo.lastUpdateDate);
            Assert.NotNull(sysInfo.firstLaunchDate);

            sysInfo.appVersion += "_v2"; // Simulate that the app was updated 

            prefs = new Preferences(store);
            Assert.Equal(firstStart, prefs.GetFirstStartDate());
            Assert.NotEqual(lastUpdate, prefs.GetLastUpdateDate());

            // PlayerPrefsV2.SetStringEncrypted and PlayerPrefsV2.GetStringDecrypted example:
            await prefs.SetStringEncrypted("mySecureString", "some text to encrypt", password: "myPassword123");
            var decryptedAgain = await prefs.GetStringDecrypted("mySecureString", null, password: "myPassword123");
            Assert.Equal("some text to encrypt", decryptedAgain);

        }

    }
}
