using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using com.csutil.model;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
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
        public IEnumerator TestPlayingAudio() {
            yield return TestPlayingAudioFromFile().AsCoroutine();
            yield return TestPlayingAudioFromUrl().AsCoroutine();
        }

        private async Task TestPlayingAudioFromFile(bool playAudioClip = false) {
            using var t = Log.MethodEntered();
            var dir = EnvironmentV2.instance.GetRootTempFolder().GetChildDir("TestPlayingAudioFromFile").CreateV2();
            // First download a sample mp3 file and store it in the dir:
            var remoteAudioData = new AudioFile() { url = "https://download.samplelib.com/mp3/sample-3s.mp3" };
            Assert.IsTrue(await remoteAudioData.DownloadTo(dir));
            var audioFile = remoteAudioData.GetFileEntry(dir.FileSystem);
            Assert.IsTrue(audioFile.IsNotNullAndExists());
            var audioClip = await audioFile.LoadAudioClip();
            await TestWithAudioClip(audioClip, playAudioClip);
        }

        private async Task TestPlayingAudioFromUrl(bool playAudioClip = false) {
            using var t = Log.MethodEntered();
            var url = "https://download.samplelib.com/mp3/sample-3s.mp3";
            if (AudioHelper.TryGetAudioTypeFor("mp3", out var audioType)) {
                var audioClip = await UnityWebRequestMultimedia.GetAudioClip(url, audioType).SendV2().GetResult<AudioClip>();
                await TestWithAudioClip(audioClip, playAudioClip);
            } else {
                throw new InvalidDataException("Failed to load audio from url=" + url);
            }
        }

        private async Task TestWithAudioClip(AudioClip audioClip, bool playAudioClip) {
            Assert.IsNotNull(audioClip);
            Assert.AreNotEqual(0, audioClip.length);
            if (playAudioClip) {
                IoC.inject.GetOrAddComponentSingleton<AudioListener>(this);
                var audioSource = new GameObject("AudioSource").AddComponent<AudioSource>();
                audioSource.clip = audioClip;
                audioSource.Play();
                var audioClipLengthInMs = (int)(audioClip.length * 1000);
                await TaskV2.Delay(audioClipLengthInMs + 100);
            }
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