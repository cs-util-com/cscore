using com.csutil.http;
using com.csutil.logging;
using StbImageLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace com.csutil.demos.demo1 {

    public class DemoScreen1 : MonoBehaviour {

        private Dictionary<string, Link> links;
        public string url = "https://picsum.photos/4000/2000";

        void Start() {

            LogConsole.RegisterForAllLogEvents(this);

            links = gameObject.GetLinkMap();
            links.Get<Button>("ButtonTestJsonLib").SetOnClickAction(delegate { TestJsonSerialization(); });
            links.Get<Button>("ButtonTestPing").SetOnClickAction(delegate {
                StartCoroutine(TestCurrentPing(links.Get<InputField>("IpInput").text));
            });
            var img = links.Get<Image>("Image1");
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            links.Get<Button>("ButtonLoadImage1").SetOnClickAction(delegate {
                var t = Log.MethodEntered("ButtonLoadImage1");
                StartCoroutine(DownloadTexture2D(new Uri(url), new Response<Texture2D>().WithResultCallback(texture2d => {
                    img.sprite = texture2d.ToSprite();
                    Log.MethodDone(t);
                })));
            });
            links.Get<Button>("ButtonLoadImage2").SetOnClickAction(delegate {
                var t = Log.MethodEntered("ButtonLoadImage2");
                StartCoroutine(DownloadBytes(new Uri(url), new Response<byte[]>().WithResultCallback(async downloadedBytes => {
                    var texture2d = await ImageHelper.ToTexture2D(downloadedBytes);
                    img.sprite = texture2d.ToSprite();
                    Log.MethodDone(t);
                })));
            });
        }

        private class MyClass1 {
            public string theCurrentTime;
            public int myInt;
        }

        private void TestJsonSerialization() {
            var prefsKey = "testObj1";
            var myObj = new MyClass1() { theCurrentTime = "It is " + DateTime.Now, myInt = 123 };
            PlayerPrefsV2.SetObject(prefsKey, myObj);
            AssertV2.AreEqual(myObj.theCurrentTime, PlayerPrefsV2.GetObject<MyClass1>(prefsKey, null).theCurrentTime);
            AssertV2.AreEqual(myObj.myInt, PlayerPrefsV2.GetObject<MyClass1>(prefsKey, null).myInt);
            links.Get<Text>("JsonOutput").text = JsonWriter.GetWriter().Write(PlayerPrefsV2.GetObject<MyClass1>(prefsKey, null));
        }

        private IEnumerator TestCurrentPing(string ipOrUrl) {
            Log.d("Will ping now ipOrUrl=" + ipOrUrl);
            var pingTask = RestFactory.instance.GetCurrentPing(ipOrUrl);
            yield return pingTask.AsCoroutine();
            links.Get<Text>("PingOutput").text = "Current Ping: " + pingTask.Result + "ms";
        }

        public static IEnumerator DownloadTexture2D(Uri self, Response<Texture2D> resp) { yield return UnityWebRequestTexture.GetTexture(self).SendWebRequestV2(resp); }

        public static IEnumerator DownloadBytes(Uri self, Response<byte[]> resp) { yield return new UnityWebRequest(self).SendWebRequestV2(resp); }

    }

    public static class ImageHelper {

        [Obsolete("Uses the StbImageLib internally and seems to be slower then loading the texture directly via UnityWebRequest")]
        public static async Task<Texture2D> ToTexture2D(byte[] downloadedBytes) { return ToTexture2D(await ToImageResult(downloadedBytes)); }

        private static Task<ImageResult> ToImageResult(byte[] bytes) {
            return TaskV2.Run(() => {
                var stream = new MemoryStream(bytes);
                var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                stream.Dispose();
                Conversion.stbi__vertical_flip(image.Data, image.Width, image.Height, 4);
                return Task.FromResult(image);
            });
        }

        public static Texture2D ToTexture2D(this ImageResult self) {
            AssertV2.AreEqual(8, self.BitsPerChannel);
            Texture2D tex = new Texture2D(self.Width, self.Height, TextureFormat.RGBA32, false);
            tex.LoadRawTextureData(self.Data);
            tex.Apply();
            return tex;
        }

    }

}