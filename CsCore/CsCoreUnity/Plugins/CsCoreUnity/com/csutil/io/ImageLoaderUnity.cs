using com.csutil.io;
using com.csutil.ui.Components;
using StbImageLib;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil {

    public static class ImageLoaderUnity {

        /// <summary>
        /// Uses the StbImageLib internally and seems to be a bit slower then loading the texture directly via UnityWebRequest but 
        /// can be performed in a background thread.
        /// </summary>
        public static async Task<Texture2D> ToTexture2D(byte[] downloadedBytes) {
            return ToTexture2D(await ImageLoader.LoadImageInBackground(downloadedBytes));
        }

        public static Texture2D ToTexture2D(this ImageResult self) {
            AssertV2.AreEqual(8, self.BitsPerChannel);
            Texture2D tex = new Texture2D(self.Width, self.Height, TextureFormat.RGBA32, false);
            tex.LoadRawTextureData(self.Data);
            tex.Apply();
            return tex;
        }

        public static async Task<Texture2D> LoadFromUrl(this Image self, string imageUrl) {
            if (imageUrl.IsNullOrEmpty()) { throw new ArgumentNullException("The passed imageUrl cant be null"); }
            self.GetComponent<LoadTexture2dTaskMono>().Destroy(); // Cancel previous task if possible
            var textureLoader = self.gameObject.AddComponent<LoadTexture2dTaskMono>();
            Texture2D texture2d = await textureLoader.LoadFromUrl(imageUrl);
            self.sprite = texture2d.ToSprite();
            textureLoader.Destroy();
            return texture2d;
        }

    }

}
