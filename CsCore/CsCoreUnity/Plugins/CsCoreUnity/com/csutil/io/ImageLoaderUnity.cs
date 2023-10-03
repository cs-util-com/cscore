using com.csutil.io;
using com.csutil.model;
using com.csutil.ui.Components;
using StbImageSharp;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zio;

namespace com.csutil {

    public static class ImageLoaderUnity {

        public static void LoadImageResult(this Image self, ImageResult img) { self.LoadTexture2D(img.ToTexture2D()); }

        public static void LoadTexture2D(this Image self, Texture2D tex) { self.sprite = tex.ToSprite(); }

        /// <summary> Uses the StbImageLib internally and seems to be a bit slower then loading the texture directly via UnityWebRequest but 
        /// can be performed in a background thread. </summary>
        public static async Task<Texture2D> ToTexture2D(byte[] downloadedBytes) {
            return ToTexture2D(await ImageLoader.LoadImageInBackground(downloadedBytes));
        }

        /// <summary> Loads a texture on the main thread using Unitys own LoadImage method.
        /// This is faster then the ToTexture2D method but can not be performed in a background thread. </summary>
        public static bool TryLoadTexture2DFast(byte[] downloadedBytes, out Texture2D texture2d, bool hasAlpha = false) {
            downloadedBytes.ThrowErrorIfNullOrEmpty("input image bytes");
            if (hasAlpha) {
                texture2d = new Texture2D(2, 2, TextureFormat.RGB565, false);
            } else {
                texture2d = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            }
            var couldBeLoaded = texture2d.LoadImage(downloadedBytes, hasAlpha);
            if (!couldBeLoaded) { return false; }
            
            // Unity has an error placeholder which is 8 x 8 pixels in size and has only 1 mipmap:
            var isErrorPlaceholder = texture2d.width == 8 && texture2d.height == 8 && texture2d.mipmapCount == 1;
            return !isErrorPlaceholder;
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
            self.GetComponentV2<LoadTexture2dTaskMono>().Destroy(); // Cancel previous task if possible
            var textureLoader = self.gameObject.GetOrAddComponent<LoadTexture2dTaskMono>();
            Texture2D texture2d = await textureLoader.LoadFromUrl(imageUrl);
            var downloadedTextureStillNeededByImage = self.GetComponentV2<LoadTexture2dTaskMono>().imageUrlCurrentlyLoadingFrom == imageUrl;
            if (downloadedTextureStillNeededByImage) {
                self.sprite = texture2d.ToSprite();
            } else {
                Log.w("The image was already downloading a new file, will ignore completed texture download", self);
                texture2d.Destroy();
                texture2d = null;
            }
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
            return await LoadAndPersistTo(imgRef, targetDir, thumbnailPixelWidth, (tex2d) => self.sprite = tex2d.ToSprite());
        }

        /// <summary> Downloads and shows the passed IFileRef and if a local thumbnail exists first loads the 
        /// thumbnail as a placeholder which is especially helpful when loading larger images </summary>
        /// <param name="imgRef"> The IFileRef that contains the url and target dir </param>
        /// <param name="targetDir"> The directory that should be used to download to </param>
        /// <param name="thumbnailPixelWidth"> The pixel width of the automatic thumbnail, e.g 256 </param>
        /// <returns> The loaded Texture2D of the image </returns>
        public static async Task<Texture2D> LoadAndPersistTo(this Renderer self, IFileRef imgRef, DirectoryEntry targetDir, int thumbnailPixelWidth) {
            return await LoadAndPersistTo(imgRef, targetDir, thumbnailPixelWidth, (tex2d) => self.material.mainTexture = tex2d);
        }

        public static async Task<Texture2D> LoadAndPersistTo(IFileRef imgRef, DirectoryEntry targetDir, int thumbnailPixelWidth, Action<Texture2D> showTexture) {
            Texture2D tempThumbTexture = null;
            if (imgRef.fileName != null) { // Try load local thumbnail as quick as possible:
                var i = targetDir.GetChild(imgRef.fileName);
                var t = i.Parent.GetChild(i.NameWithoutExtension + ".thm");
                tempThumbTexture = await LoadThumbnailIfFound(showTexture, t, tempThumbTexture);
            }

            var fileWasDownloaded = await imgRef.DownloadTo(targetDir, useAutoCachedFileRef: true);

            var imageFile = imgRef.GetFileEntry(targetDir.FileSystem);
            FileEntry thumbnailFile = GetThumbnailFile(imgRef, targetDir);
            if (!fileWasDownloaded && tempThumbTexture == null) {
                // If there was no new file downloaded and the thumbnail is available and not loaded yet:
                tempThumbTexture = await LoadThumbnailIfFound(showTexture, thumbnailFile, tempThumbTexture);
            }
            if (!imageFile.Exists) { return null; }

            // Load the full image as a texture and show it in the UI:
            Texture2D texture2d = await imageFile.LoadTexture2D();
            showTexture(texture2d);
            if (tempThumbTexture != null) { tempThumbTexture.Destroy(true); }

            if (fileWasDownloaded || !thumbnailFile.Exists) { // Store a thumbnail file if needed:
                Texture2D thumbnail = texture2d.CopyTexture();
                thumbnail.ResizeV2(thumbnailPixelWidth);
                if (imgRef.IsJpgFile()) {
                    thumbnail.SaveToJpgFile(thumbnailFile);
                } else {
                    thumbnail.SaveToPngFile(thumbnailFile);
                }

                thumbnail.Destroy(true);
            }
            return texture2d;
        }

        private static FileEntry GetThumbnailFile(IFileRef self, DirectoryEntry targetDir) {
            var imageFile = self.GetFileEntry(targetDir.FileSystem);
            return imageFile.Parent.GetChild(imageFile.NameWithoutExtension + ".thm");
        }

        private static async Task<Texture2D> LoadThumbnailIfFound(Action<Texture2D> showTexture, FileEntry thumbnailFile, Texture2D tempThumbTexture) {
            if (thumbnailFile.Exists) {
                if (tempThumbTexture != null) { tempThumbTexture.Destroy(true); }
                tempThumbTexture = await thumbnailFile.LoadTexture2D();
                showTexture(tempThumbTexture);
            }
            return tempThumbTexture;
        }
    }

}
