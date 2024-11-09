using com.csutil.system;
using System;
using System.Diagnostics;
using Xunit;
using Zio;

namespace com.csutil.tests.system {

    public class SymLinkTests {

        public SymLinkTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {
            var root = EnvironmentV2.instance.GetOrAddTempFolder("SymLinkTests");
            foreach (var dir in root.GetDirectories()) { dir.DeleteV2(); }

            var f1 = root.CreateSubdirectory("Folder1");
            var targetForF1 = root.GetChildDir("TargetForFolder1");

            SymLinker.CreateSymlink(f1, targetForF1);

            const string fileName = "t1.txt";
            const string text = "Abc";
            f1.GetChild(fileName).SaveAsText(text);

            // Check that the change in f1 also shows in the symlinked TargetForFolder1
            Assert.Equal(text, targetForF1.GetChild(fileName).LoadAs<string>(null));
        }

    }

}