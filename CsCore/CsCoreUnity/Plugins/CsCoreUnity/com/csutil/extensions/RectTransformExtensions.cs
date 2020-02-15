using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil {

    public static class RectTransformExtensions {

        public static void SetWidth(this RectTransform self, float width) {
            self.sizeDelta = self.sizeDelta.SetX(width);
        }

        public static void SetHeight(this RectTransform self, float height) {
            self.sizeDelta = self.sizeDelta.SetY(height);
        }

        public static Vector3[] GetWorldCornersV2(this RectTransform self, Vector3[] cache = null) {
            if (cache == null) { cache = new Vector3[4]; }
            AssertV2.AreEqual(4, cache.Length);
            self.GetWorldCorners(cache);
            return cache;
        }

        public static Bounds GetWorldBounds(this RectTransform self, Vector3[] cache = null) {
            var corners = self.GetWorldCornersV2();
            var height = (corners[0] - corners[1]).magnitude;
            var width = (corners[1] - corners[2]).magnitude;
            return new Bounds(self.position, size: new Vector3(width, height, 0));
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
            return self.GetComponentInParent<Canvas>().rootCanvas;
        }

    }

}
