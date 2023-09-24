using Xunit;
using Zio;

namespace com.csutil.integrationTests.io {
    
    public class FileEntryIntegrationTests {

        [Fact]
        public void TestVeryLongFilePathes() {
            var dir = CreateDirectoryForTesting("TestVeryLongFilePathes");
            for (int i = 0; i < 100; i++) { dir = dir.GetChildDir("Abcdefghijlm"); }
            Assert.True(dir.FullName.Length > 260, "dir.FullName=" + dir.FullName);
            dir.CreateV2();
            Assert.True(dir.Exists);

            var file = dir.GetChild("test.txt");
            file.SaveAsText("Abc");
            Assert.True(file.Exists);
            Log.d("file: " + file.FullName);
        }
        
        private static DirectoryEntry CreateDirectoryForTesting(string name) {
            var rootDir = EnvironmentV2.instance.GetRootTempFolder().GetChildDir(name);
            rootDir.DeleteV2(); // ensure that the test folder does not yet exist
            rootDir.CreateV2();
            Assert.True(rootDir.Exists);
            return rootDir;
        }
        
    }
    
}