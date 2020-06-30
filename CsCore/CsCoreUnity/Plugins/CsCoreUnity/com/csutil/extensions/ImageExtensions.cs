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

        public static bool ResizeV2(this Texture2D self, float width, float height = 0, bool hasMipMap = true, FilterMode filter = FilterMode.Bilinear,
                                                               float horCropCenter = 0.5f, float vertCropCenter = 0.5f, float horF = 1, float vertF = 1) {

            if (width == 0 && height == 0) { throw new ArgumentException("Either height or width have to be set, both 0"); }
            var aspect = self.width / (float)self.height;
            if (width == 0) { width = (height * aspect); }
            var width2 = width;
            var height2 = (width / aspect);
            if (height == 0) { height = height2; }
            if (width2 < width) {
                height2 = height2 * (width / width2);
                width2 = width;
            }
            if (height2 < height) {
                width2 = width2 * (height / height2);
                height2 = height;
            }

            width2 /= horF;
            height2 /= vertF;

            RenderTexture tempRenderTex = RenderTexture.GetTemporary((int)width2, (int)height2, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            try {
                RenderTexture.active = tempRenderTex;
                Graphics.Blit(self, tempRenderTex);
                self.filterMode = filter;
                if (!self.Resize((int)width, (int)height, self.format, hasMipMap)) { return false; }

                var wStartPoint = (width2 - width) * horCropCenter;
                if (wStartPoint < 0) { wStartPoint = 0; }

                var hStartPoint = (height2 - height) * vertCropCenter;
                if (hStartPoint < 0) { hStartPoint = 0; }

                self.ReadPixels(new Rect(wStartPoint, hStartPoint, width2, height2), 0, 0);
                self.Apply();
                return true;
            }
            finally { RenderTexture.ReleaseTemporary(tempRenderTex); }
        }
    }

}
