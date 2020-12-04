using NUnit.Framework;
using UnityEngine;

namespace com.csutil.tests.extensions {

    public class ColorTests {

        [Test]
        public void TestColorToHsvToColor() {
            AssertToHsvAndBack(Color.red);
            AssertToHsvAndBack(Color.green);
            AssertToHsvAndBack(Color.blue);
            AssertToHsvAndBack32(Color.red);
            AssertToHsvAndBack32(Color.green);
            AssertToHsvAndBack32(Color.blue);
        }

        [Test]
        public void TestGetContrastColor() {
            Assert.AreEqual(Color.white, Color.black.GetContrastBlackOrWhite());
            Assert.AreEqual(Color.black, Color.white.GetContrastBlackOrWhite());
        }

        [Test]
        public void TestBrightness() {
            Assert.AreEqual(1, Color.white.GetBrightness());
            Assert.AreEqual(0, Color.black.GetBrightness());
            Assert.AreEqual(1, ((Color32)Color.white).GetBrightness());
            Assert.AreEqual(0, ((Color32)Color.black).GetBrightness());
        }

        private static void AssertToHsvAndBack(Color original) {
            var hsv = original.ToHsv();
            Color converted = ColorUtil.HsvToColor(hsv);
            Assert.AreEqual(original.r, converted.r);
            Assert.AreEqual(original.g, converted.g);
            Assert.AreEqual(original.b, converted.b);
            Assert.AreEqual(original.a, converted.a);
        }

        private static void AssertToHsvAndBack32(Color32 original32) {
            var hsv = original32.ToHsv();
            Color32 converted32 = ColorUtil.HsvToColor32(hsv);
            Assert.AreEqual(original32.r, converted32.r);
            Assert.AreEqual(original32.g, converted32.g);
            Assert.AreEqual(original32.b, converted32.b);
            Assert.AreEqual(original32.a, converted32.a);
        }

    }

}