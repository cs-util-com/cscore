﻿using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil {

    public static class RectTransformExtensions {

        public static void SetWidth(this RectTransform self, float width) {
            self.sizeDelta = self.sizeDelta.SetX(width);
        }

        public static void SetHeight(this RectTransform self, float height) {
            self.sizeDelta = self.sizeDelta.SetY(height);
        }

        public static float GetWidth(this RectTransform self) { return self.rect.width; }
        public static float GetHeight(this RectTransform self) { return self.rect.height; }
        public static Vector2 GetSize(this RectTransform self) { return self.rect.size; }

        public static Vector3[] GetWorldCornersV2(this RectTransform self, Vector3[] cache = null) {
            if (cache == null) { cache = new Vector3[4]; }
            AssertV3.AreEqual(4, cache.Length);
            self.GetWorldCorners(cache);
            return cache;
        }

        public static Bounds GetWorldBounds(this RectTransform self, Vector3[] cornersCache = null) {
            var corners = self.GetWorldCornersV2(cornersCache);
            return new Bounds(self.position, corners[2] - corners[0]);
        }

        public static bool IsVisibleInScreen(this RectTransform self, Vector3[] cache = null) {
            return self.IsVisibleInRect(ScreenV2.GetScreenRect(), cache);
        }

        public static bool IsVisibleInRect(this RectTransform self, Rect rect, Vector3[] cache = null) {
            var corners = self.GetWorldCornersV2(cache);
            foreach (var corner in corners) { if (rect.Contains(corner)) { return true; } }
            if (self.ContainsScreenPoint(new Vector2(rect.xMin, rect.yMin))) { return true; }
            if (self.ContainsScreenPoint(new Vector2(rect.xMin, rect.yMax))) { return true; }
            if (self.ContainsScreenPoint(new Vector2(rect.xMax, rect.yMin))) { return true; }
            if (self.ContainsScreenPoint(new Vector2(rect.xMax, rect.yMax))) { return true; }
            if (self.ContainsScreenPoint(rect.center)) { return true; }
            return false;
        }

        public static bool ContainsScreenPoint(this RectTransform self, Vector2 screenPoint) {
            return RectTransformUtility.RectangleContainsScreenPoint(self, screenPoint);
        }

        public static Canvas GetRootCanvas(this RectTransform self) {
            return self.GetComponentInParent<Canvas>()?.rootCanvas;
        }

        public static RectTransform SetPadding(this RectTransform self, float paddingInPixels) {
            return self.SetPadding(paddingInPixels, paddingInPixels, paddingInPixels, paddingInPixels);
        }

        public static RectTransform SetPadding(this RectTransform self, float left, float right, float top, float bottom) {
            self.offsetMin = new Vector2(left, bottom);
            self.offsetMax = new Vector2(-right, -top);
            return self;
        }

        // Top-level UIs in the view stack should always fill their parent
        public static void SetAnchorsStretchStretch(this RectTransform self) {
            self.anchorMin = new Vector2(0, 0);
            self.anchorMax = new Vector2(1, 1);
            self.pivot = new Vector2(0.5f, 0.5f);
            self.SetPadding(0);
        }

        /// <summary> The input system has its coordinate system in the bottom left of the screen, so 
        /// any element placed or moved based on the input system values should also use the 
        /// bottom left as the 0,0 position to simplify any math used afterwards </summary>
        public static bool SetAnchorsBottomLeft(this RectTransform self) {
            if (self.anchorMin == Vector2.zero && self.anchorMax == Vector2.zero) { return false; }
            self.anchorMin = Vector2.zero;
            self.anchorMax = Vector2.zero;
            return true;
        }

        public static float GetVerticalPercentOnScreen(this RectTransform self, Camera cachedCam, Vector3[] cachedCorners) {
            if (cachedCorners == null) { cachedCorners = new Vector3[4]; }
            var prtBounds = self.GetWorldBounds(cachedCorners);
            if (cachedCam == null) { cachedCam = self.GetRootCanvas()?.worldCamera; }
            var bottomCorner = RectTransformUtility.WorldToScreenPoint(cachedCam, cachedCorners[2]);
            var totalHeightInPixels = ScreenV2.height + prtBounds.extents.y * 2;
            var progressInPixels = Mathf.Min(Mathf.Max(0, bottomCorner.y), totalHeightInPixels);
            return progressInPixels / totalHeightInPixels;
        }

    }

}