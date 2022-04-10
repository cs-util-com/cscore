using com.csutil.http;
using com.csutil.logging;
using com.csutil.model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zio;

namespace com.csutil.tests {

    public class Ui14_ImageLoading : UnitTestMono {

        public string testUrl = "https://github.com/cs-util-com/cscore/raw/master/CsCore/assets/logo-cscore1024x1024_2.png";

        private Dictionary<string, Link> links;

        public override IEnumerator RunTest() {

            LogConsole.RegisterForAllLogEvents(this);

            links = gameObject.GetLinkMap();

            TestTexture2dVsRawByteLoadingSpeeds();

            links.Get<Button>("ButtonLoadImage3").SetOnClickAction(async delegate {
                var t = Log.MethodEntered("ButtonLoadImage3");
                Texture2D texture2d = await links.Get<Image>("Image2").LoadFromUrl(GetUrl());
                Log.MethodDone(t);
                Toast.Show($"The loaded texture has the size: {texture2d.width}x{texture2d.height} pixels");
            });

            links.Get<Button>("ButtonLoadImage4").SetOnClickAction(async delegate {
                DirectoryEntry targetDir = EnvironmentV2.instance.GetOrAddTempFolder("Ui14_ImageLoading");

                var imgRefFile = targetDir.GetChild("imgRef.txt");
                FileRef imgRef = imgRefFile.Exists ? imgRefFile.LoadAs<FileRef>() : null;
                if (imgRef == null) { imgRef = new FileRef() { url = testUrl }; }

                var t = Log.MethodEntered("LoadAndPersistTo");
                await links.Get<Image>("Image2").LoadAndPersistTo(imgRef, targetDir, 64);
                Log.MethodDone(t);

                imgRefFile.SaveAsJson(imgRef, true); // Save so that it will be reused next time
            });

            yield return null;
        }

        private void TestTexture2dVsRawByteLoadingSpeeds() {
            var img = links.Get<Image>("Image1");
            img.type = Image.Type.Simple;
            img.preserveAspect = true;

            links.Get<Button>("ButtonLoadImage1").SetOnClickAction(delegate {
                var t = Log.MethodEntered("ButtonLoadImage1");
                StartCoroutine(DownloadTexture2D(new Uri(GetUrl()), new Response<Texture2D>().WithResultCallback(texture2d => {
                    img.sprite = texture2d.ToSprite();
                    Log.MethodDone(t);
                })));
            });

            links.Get<Button>("ButtonLoadImage2").SetOnClickAction(delegate {
                var t = Log.MethodEntered("ButtonLoadImage2");
                StartCoroutine(DownloadBytes(new Uri(GetUrl()), new Response<byte[]>().WithResultCallback(async downloadedBytes => {
                    Texture2D texture2d = await ImageLoaderUnity.ToTexture2D(downloadedBytes);
                    img.sprite = texture2d.ToSprite();
                    Log.MethodDone(t);
                })));
            });
        }

        /// <summary> Get URL string from the user input field in the UI </summary>
        private string GetUrl() { return links.Get<InputField>("UrlToLoadInput").text; }

        public static IEnumerator DownloadTexture2D(Uri self, Response<Texture2D> resp) {
            yield return UnityWebRequestTexture.GetTexture(self).SendWebRequestV2(resp);
        }

        public static IEnumerator DownloadBytes(Uri self, Response<byte[]> resp) {
            yield return new UnityWebRequest(self).SendWebRequestV2(resp);
        }

        private class FileRef : IFileRef {
            public string dir { get; set; }
            public string fileName { get; set; }
            public string url { get; set; }
            public Dictionary<string, object> checksums { get; set; }
            public string mimeType { get; set; }
        }

    }

}