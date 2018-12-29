using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http;
using Xunit;

namespace com.csutil.tests {
    public class FileTests : IDisposable {

        public FileTests() { // Setup before each test
        }

        public void Dispose() { // TearDown after each test
        }

        [Fact]
        public async Task TestFileLoading() {
            var env = EnvironmentV2.instance;
            DirectoryInfo dir = null;
            Assert.True(dir.IsNullOrDoesNotExist());
            dir = env.GetCurrentDirectory();
            Assert.False(dir.IsNullOrDoesNotExist());
            dir = EnvironmentV2.instance.GetAppDataFolder();
            Log.d("dir=" + dir.FullPath());
            Assert.False(dir.IsNullOrDoesNotExist());



        }


    }
}