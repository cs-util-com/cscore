using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.csutil.tests.io {

    public class PlayerPrefsV2Tests {

        [Test]
        public void TestGetAndSetBool() {
            var key = "b1";
            Assert.IsFalse(PlayerPrefsV2.GetBool(key, false));
            PlayerPrefsV2.SetBool(key, true);
            Assert.IsTrue(PlayerPrefsV2.GetBool(key, false));
            PlayerPrefsV2.DeleteKey(key);
        }

        [Test]
        public void TestGetAndSetEncyptedString() {
            var key = "b1";
            var value = "val 1";
            var password = "1234";
            PlayerPrefsV2.DeleteKey(key);
            Assert.AreEqual(null, PlayerPrefsV2.GetStringDecrypted(key, null, password));
            PlayerPrefsV2.SetStringEncrypted(key, value, password);
            Assert.AreEqual(value, PlayerPrefsV2.GetStringDecrypted(key, null, password));
            Assert.AreNotEqual(value, PlayerPrefsV2.GetStringDecrypted(key, null, "incorrect password"));
            Assert.AreNotEqual(value, PlayerPrefsV2.GetString(key, null));
            PlayerPrefsV2.DeleteKey(key);
        }

        [Test]
        public void TestGetAndSetComplexObjects() {
            var key = "b1";
            var myObj = new MyClass1() { s = "aaa", i = 123 };

            Assert.AreEqual(null, PlayerPrefsV2.GetObject<MyClass1>(key, null));
            PlayerPrefsV2.SetObject(key, myObj);
            Assert.AreEqual(myObj.s, PlayerPrefsV2.GetObject<MyClass1>(key, null).s);
            Assert.AreEqual(myObj.i, PlayerPrefsV2.GetObject<MyClass1>(key, null).i);
            PlayerPrefsV2.DeleteKey(key);
        }

        private class MyClass1 {
            public string s;
            public int i;
        }

    }

}
