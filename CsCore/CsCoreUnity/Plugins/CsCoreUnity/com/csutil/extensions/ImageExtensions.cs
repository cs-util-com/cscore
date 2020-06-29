using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zio;

namespace com.csutil {

    public static class ImageExtensions {

        public static Sprite ToSprite(this Texture2D self) {
            return Sprite.Create(self, new Rect(0, 0, self.width, self.height), new Vector2(0.5f, 0.5f));
        }

        public static async Task<Texture2D> LoadTexture2D(this FileEntry self) {
            return await ImageLoaderUnity.ToTexture2D(self.ReadAllBytes());
        }

        public static void SaveToJpgFile(this Texture2D self, FileEntry targetFile, int quality = 90) {
            if (!targetFile.Parent.Exists) { targetFile.Parent.CreateV2(); }
            targetFile.SaveStream(new MemoryStream(self.EncodeToJPG(quality)));
        }

        [Obsolete("Does not capture UI, consider using ScreenCapture.CaptureScreenshotAsTexture instead")]
        public static Texture2D CaptureScreenshot(this Camera self, int width = 0, int height = 0) {
            return CaptureScreenshot(new Camera[] { self }, width, height);
        }

        [Obsolete("Does not capture UI, consider using ScreenCapture.CaptureScreenshotAsTexture instead")]
        public static Texture2D CaptureScreenshot(this Camera[] cameras, int width = 0, int height = 0) {
            if (width == 0) { width = ScreenV2.width; }
            if (height == 0) { height = width * ScreenV2.height / ScreenV2.width; }
            Texture2D texture2d = new Texture2D(width, height, TextureFormat.RGB24, false);
            RenderTexture renderTexture = new RenderTexture(texture2d.width, texture2d.height, 24);

            foreach (Camera self in cameras) {
                RenderTexture prev = self.targetTexture;
                self.targetTexture = renderTexture;
                self.Render();
                self.targetTexture = prev;
            }

            RenderTexture.active = renderTexture;
            texture2d.ReadPixels(new Rect(0, 0, texture2d.width, texture2d.height), 0, 0);
            texture2d.Apply();

            RenderTexture.active = null;
            renderTexture.Destroy();
            return texture2d;
        }

        public static Texture2D CopyTexture(this Texture2D self) {
            var target = new Texture2D(self.width, self.height, self.format, self.mipmapCount > 1);
            var width = self.width;
            var height = self.height;
            var srcX = 0;
            var srcY = 0;
            if (SystemInfo.copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.None) {
                target.SetPixels(self.GetPixels(srcX, srcY, width, height));
            } else {
                Graphics.CopyTexture(self, 0, 0, srcX, srcY, width, height, target, 0, 0, 0, 0);
            }
            return target;
        }

        public static bool ResizeV2(this Texture2D self, int width, int height = 0, bool hasMipMap = true, FilterMode filter = FilterMode.Bilinear) {

            if (width == 0 && height == 0) { throw new ArgumentException("Either height or width have to be set, both 0"); }
            var aspect = self.width / (float)self.height;
            if (width == 0) { width = (int)(height * aspect); }
            var width2 = width;
            var height2 = (int)(width / aspect);
            if (height == 0) { height = height2; }
            if (width2 < width) {
                height2 = height2 * (width / width2);
                width2 = width;
            }
            if (height2 < height) {
                width2 = width2 * (height / height2);
                height2 = height;
            }

            RenderTexture tempRenderTex = RenderTexture.GetTemporary(width2, height2, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            try {
                RenderTexture.active = tempRenderTex;
                Graphics.Blit(self, tempRenderTex);
                self.filterMode = filter;
                if (!self.Resize(width, height, self.format, hasMipMap)) { return false; }

                var wCenter = width2 / 2;
                var wStartPoint = wCenter - width / 2;
                if (wStartPoint < 0) { wStartPoint = 0; }

                var hCenter = height2 / 2;
                var hStartPoint = hCenter - height / 2;
                if (hStartPoint < 0) { hStartPoint = 0; }

                self.ReadPixels(new Rect(wStartPoint, hStartPoint, width2, height2), 0, 0);
                self.Apply();
                return true;
            }
            finally { RenderTexture.ReleaseTemporary(tempRenderTex); }
        }

    }

}
