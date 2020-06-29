using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    public class Ui20_ImageResizing : UnitTestMono {

        private Dictionary<string, Link> map;
        public string imageFileName { get { return map.Get<InputField>("imageFileName").text; } }
        public int width { get { return int.Parse(map.Get<InputField>("width").text); } }
        public int height { get { return int.Parse(map.Get<InputField>("height").text); } }

        public override IEnumerator RunTest() {
            map = gameObject.GetLinkMap();

            map.Get<Button>("Button1").SetOnClickAction(async delegate {
                Texture2D image = await LoadImage();
                image.ResizeV2(width, 0); // automatically calculate the height
                map.Get<Image>("Image1").sprite = image.ToSprite();
            });

            map.Get<Button>("Button2").SetOnClickAction(async delegate {
                Texture2D image = await LoadImage();
                image.ResizeV2(0, height); // automatically calculate the width
                map.Get<Image>("Image2").sprite = image.ToSprite();
            });

            map.Get<Button>("Button3").SetOnClickAction(async delegate {
                Texture2D image = await LoadImage();
                image.ResizeV2(width, height); // crop the center
                map.Get<Image>("Image3").sprite = image.ToSprite();
            });

            map.Get<Button>("Button4").SetOnClickAction(async delegate {
                Texture2D image = await LoadImage();
                image.ResizeV2(width, height, horCropCenter: 1, vertCropCenter: 1, horF: 0.4f, vertF: 0.4f);
                map.Get<Image>("Image4").sprite = image.ToSprite();
            });

            yield return null;

        }

        private async Task<Texture2D> LoadImage() {
            var imgFile = EnvironmentV2.instance.GetCurrentDirectory().GetChild(imageFileName);
            if (!imgFile.Exists) { throw Log.e("Missing " + imgFile.GetFullFileSystemPath()); }
            return await imgFile.LoadTexture2D();
        }

    }

}
