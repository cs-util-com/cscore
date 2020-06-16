using System.IO;
using UnityEngine;
using Zio;

namespace com.csutil {

    public static class ImageExtensions {

        public static Sprite ToSprite(this Texture2D self) { return Sprite.Create(self, new Rect(0, 0, self.width, self.height), new Vector2(0.5f, 0.5f)); }

        public static void SaveToFile(this Texture2D self, FileEntry targetFile, int quality = 90) {
            if (!targetFile.Parent.Exists) { targetFile.Parent.CreateV2(); }
            targetFile.SaveStream(new MemoryStream(self.EncodeToJPG(quality)));
        }

        public static Texture2D CaptureScreenshot(this Camera self, int width) {
            return self.CaptureScreenshot(width, (ScreenV2.height / ScreenV2.width) * width);
        }

        public static Texture2D CaptureScreenshot(this Camera self, int width, int height) {
            RenderTexture renderTexture = new RenderTexture(width, height, 24);
            self.targetTexture = renderTexture;
            Texture2D texture2d = new Texture2D(width, height, TextureFormat.RGB24, false);
            self.Render();
            RenderTexture.active = renderTexture;
            texture2d.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            self.targetTexture = null;
            RenderTexture.active = null;
            renderTexture.Destroy();
            return texture2d;
        }

    }

}
