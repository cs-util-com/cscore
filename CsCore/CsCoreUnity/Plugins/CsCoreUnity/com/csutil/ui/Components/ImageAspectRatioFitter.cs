using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.Components {

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(AspectRatioFitter))]
    class ImageAspectRatioFitter : MonoBehaviour {

        /// <summary> Set this to 0 to disable vertical parallax </summary>
        [Range(-1, 1)]
        public float verticalParallaxStrength = -0.6f;

        private Image img;
        private AspectRatioFitter aspectRatioFitter;
        private RectTransform rt;
        private RectTransform parentRt;
        private Camera cachedCamera;
        private Vector3[] parentRtCorners = new Vector3[4];
        private Sprite lastSprite;

        private void OnEnable() {
            img = GetComponent<Image>();
            aspectRatioFitter = GetComponent<AspectRatioFitter>();
            rt = GetComponent<RectTransform>();
            parentRt = transform.parent as RectTransform;
            this.ExecuteRepeated(Refresh, delayInMsBetweenIterations: 5);
        }

        private bool Refresh() {
            var sprite = img.sprite;
            if (sprite != lastSprite) {
                lastSprite = sprite;
                var imageSizeInPixels = sprite.textureRect.size;
                aspectRatioFitter.aspectRatio = imageSizeInPixels.x / imageSizeInPixels.y;
            }
            if (verticalParallaxStrength != 0) {
                if (cachedCamera == null) { cachedCamera = parentRt.GetRootCanvas()?.worldCamera; }
                float verticalPecent = parentRt.GetVerticalPercentOnScreen(cachedCamera, parentRtCorners);
                float height = rt.sizeDelta.y;
                float strength = height < ScreenV2.height ? verticalParallaxStrength : -verticalParallaxStrength;
                float offset = -Mathf.Sign(strength) * height / 2f;
                // at 50% it has to be at localPosition=0, 0.5 / 2 (for half height) cancels out to 1:
                rt.localPosition = rt.localPosition.SetY(offset + height * verticalPecent * strength);
            }
            return true;
        }

    }

}
