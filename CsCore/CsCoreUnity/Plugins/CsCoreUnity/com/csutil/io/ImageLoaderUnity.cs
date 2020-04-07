using StbImageLib;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.io {

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

    }

}
