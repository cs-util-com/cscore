using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.model;
using NUnit.Framework;
using UnityEngine.TestTools;
using Zio;

namespace com.csutil.tests.io {

    public class FileTests {

        [SetUp]
        public void BeforeEachTest() { }

        [TearDown]
        public void AfterEachTest() { }

        [Test]
        public void TestFilesWithEnumeratorPasses() {
            var dir = EnvironmentV2.instance.GetRootTempFolder();
            Log.d("dir=" + dir.FullName);
            Assert.IsNotEmpty(dir.FullName);
            dir = EnvironmentV2.instance.GetOrAddAppDataFolder("MyApp1");
            Log.d("dir=" + dir.FullName);
            Assert.IsNotEmpty(dir.FullName);
        }

        [UnityTest]
        public IEnumerator TestLoadingAudioFromFile() {
            yield return TestLoadingAudioFromFileTask().AsCoroutine();
        }

        public async Task TestLoadingAudioFromFileTask() {
            var dir = EnvironmentV2.instance.GetRootTempFolder().GetChildDir("TestLoadingAudioFromFile").CreateV2();
            // First download a sample mp3 file and store it in the dir:
            var remoteAudioData = new AudioFile() { url = "https://download.samplelib.com/mp3/sample-3s.mp3" };
            Assert.IsTrue(await remoteAudioData.DownloadTo(dir));
            var audioFile = remoteAudioData.GetFileEntry(dir.FileSystem);
            Assert.IsTrue(audioFile.IsNotNullAndExists());
            var audioClip = await audioFile.LoadAudioClip();
            Assert.IsNotNull(audioClip);
        }

        private class AudioFile : IFileRef {
            public string dir { get; set; }
            public string fileName { get; set; }
            public string url { get; set; }
            public Dictionary<string, object> checksums { get; set; }
            public string mimeType { get; set; }
        }

    }

}