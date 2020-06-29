using com.csutil.io;
using com.csutil.model;
using com.csutil.ui.Components;
using StbImageLib;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zio;

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

        /// <summary> Downloads and shows the passed IFileRef and if a local thumbnail exists first loads the 
        /// thumbnail as a placeholder which is especially helpful when loading larger images </summary>
        /// <param name="imgRef"> The IFileRef that contains the url and target dir </param>
        /// <param name="targetDir"> The directory that should be used to download to </param>
        /// <param name="thumbnailPixelWidth"> The pixel width of the automatic thumbnail, e.g 256 </param>
        /// <returns> The loaded Texture2D of the image </returns>
        public static async Task<Texture2D> LoadAndPersistTo(this Image self, IFileRef imgRef, DirectoryEntry targetDir, int thumbnailPixelWidth) {
            Texture2D tempThumbTexture = null;
            if (imgRef.fileName != null) { // Try load local thumbnail as quick as possible:
                var i = targetDir.GetChild(imgRef.fileName);
                var t = i.Parent.GetChild(i.NameWithoutExtension + ".thm");
                tempThumbTexture = await LoadThumbnailIfFound(self, t, tempThumbTexture);
            }

            var fileWasDownloaded = await imgRef.DownloadTo(targetDir);

            var imageFile = imgRef.GetFileEntry(targetDir.FileSystem);
            var thumbnailFile = imageFile.Parent.GetChild(imageFile.NameWithoutExtension + ".thm");
            if (!fileWasDownloaded && tempThumbTexture == null) {
                // If there was no new file downloaded and the thumbnail is available and not loaded yet:
                tempThumbTexture = await LoadThumbnailIfFound(self, thumbnailFile, tempThumbTexture);
            }
            if (!imageFile.Exists) { return null; }

            // Load the full image as a texture and show it in the UI:
            Texture2D texture2d = await imageFile.LoadTexture2D();
            self.sprite = texture2d.ToSprite();
            if (tempThumbTexture != null) { tempThumbTexture.Destroy(true); }

            if (fileWasDownloaded || !thumbnailFile.Exists) { // Store a thumbnail file if needed:
                Texture2D thumbnail = texture2d.CopyTexture();
                thumbnail.ResizeV2(thumbnailPixelWidth);
                thumbnail.SaveToJpgFile(thumbnailFile);
                thumbnail.Destroy(true);
            }
            return texture2d;
        }

        private static async Task<Texture2D> LoadThumbnailIfFound(Image self, FileEntry thumbnailFile, Texture2D tempThumbTexture) {
            if (thumbnailFile.Exists) {
                if (tempThumbTexture != null) { tempThumbTexture.Destroy(true); }
                tempThumbTexture = await thumbnailFile.LoadTexture2D();
                self.sprite = tempThumbTexture.ToSprite();
            }
            return tempThumbTexture;
        }
    }

}
