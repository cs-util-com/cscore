using com.csutil.http;
using com.csutil.io;
using com.csutil.logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace com.csutil.tests {

    public class Ui14_ImageLoading : UnitTestMono {

        private Dictionary<string, Link> links;

        public override IEnumerator RunTest() {

            LogConsole.RegisterForAllLogEvents(this);

            links = gameObject.GetLinkMap();
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
                    var texture2d = await ImageLoaderUnity.ToTexture2D(downloadedBytes);
                    img.sprite = texture2d.ToSprite();
                    Log.MethodDone(t);
                })));
            });
            links.Get<Button>("ButtonLoadImage3").SetOnClickAction(async delegate {
                var t = Log.MethodEntered("ButtonLoadImage3");
                Texture2D loadedImage = await links.Get<Image>("Image2").LoadFromUrl(GetUrl());
                Log.MethodDone(t);
                Toast.Show($"The loaded texture has the size: {loadedImage.width}x{loadedImage.height} pixels");
            });

            yield return null;

        }

        private string GetUrl() { return links.Get<InputField>("UrlToLoadInput").text; }

        public static IEnumerator DownloadTexture2D(Uri self, Response<Texture2D> resp) { yield return UnityWebRequestTexture.GetTexture(self).SendWebRequestV2(resp); }

        public static IEnumerator DownloadBytes(Uri self, Response<byte[]> resp) { yield return new UnityWebRequest(self).SendWebRequestV2(resp); }

    }

}