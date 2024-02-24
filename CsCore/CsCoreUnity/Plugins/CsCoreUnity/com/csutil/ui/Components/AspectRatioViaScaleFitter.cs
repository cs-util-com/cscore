using UnityEngine;

namespace com.csutil.ui.Components {

    /// <summary> Keeps the width and height of a UI element at fixed values and only modifies the
    /// scale of the element to stay in sync with the relative size to a target transform </summary>
    [RequireComponent(typeof(RectTransform))]
    public class AspectRatioViaScaleFitter : MonoBehaviour {

        /// <summary> The relative size of the element to the target transform, if set to zero will
        /// automatically compute based on the target transform </summary>
        public Vector2 relativeSize = Vector2.zero;

        /// <summary> The transform to take the size from, if not set, will automatically use the
        /// parent RectTransform </summary>
        public RectTransform targetTransform;

        /// <summary> If true the rescaling will always happen uniformly, if false the aspect ratio of the
        /// element will be changed to match the target transform </summary>
        public bool keepOriginalAspectRatio = true;

        private RectTransform rectTransform;

        void Update() {
            AdjustScaleToFitFixedSize();
        }

        private void AdjustScaleToFitFixedSize() {
            if (rectTransform == null) { rectTransform = GetComponent<RectTransform>(); }
            if (targetTransform == null) { targetTransform = transform.parent.GetComponent<RectTransform>(); }
            if (relativeSize == Vector2.zero) {
                var scale = rectTransform.localScale;
                // use the targetTransform and rectTransform to calculate the current relative size:
                relativeSize = new Vector2(rectTransform.rect.size.x / targetTransform.rect.size.x * scale.x,
                                           rectTransform.rect.size.y / targetTransform.rect.size.y * scale.y);
            }

            // Get the size of the target transform
            Vector2 targetSize = targetTransform.rect.size;
            // Calculate the desired size based on the relative size
            Vector2 desiredSize = new Vector2(targetSize.x * relativeSize.x, targetSize.y * relativeSize.y);
            Vector2 currentSize = rectTransform.rect.size;
            if (keepOriginalAspectRatio) {
                // Calculate uniform scale factor based on the smallest relative dimension
                float scaleFactor = Mathf.Min(desiredSize.x / currentSize.x, desiredSize.y / currentSize.y);
                var newLocalScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                // if new values are different to old values apply the new scale:
                if (newLocalScale != rectTransform.localScale) {
                    rectTransform.localScale = newLocalScale;
                }
            } else {
                // Calculate independent scale factors for width and height
                Vector2 scaleFactor = new Vector2(desiredSize.x / currentSize.x, desiredSize.y / currentSize.y);
                var newLocalScale = new Vector3(scaleFactor.x, scaleFactor.y, rectTransform.localScale.z);
                if (newLocalScale != rectTransform.localScale) {
                    rectTransform.localScale = newLocalScale;
                }
            }
        }

    }

}