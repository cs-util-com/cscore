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
            DirectoryInfo dir = EnvironmentV2.instance.GetCurrentDirectory();
            Log.d("dir=" + dir.FullPath());
            dir = EnvironmentV2.instance.GetAppDataFolder();
            Log.d("dir=" + dir.FullPath());

        }


    }
}