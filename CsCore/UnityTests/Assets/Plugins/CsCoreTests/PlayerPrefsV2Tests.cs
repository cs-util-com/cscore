using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.csutil.tests.io {

    public class PlayerPrefsV2Tests {

        [Test]
        public void ExampleUsage() {

            // PlayerPrefsV2.SetBool and PlayerPrefsV2.GetBool example:
            bool myBool = true;
            PlayerPrefsV2.SetBool("myBool", myBool);
            Assert.AreEqual(myBool, PlayerPrefsV2.GetBool("myBool", defaultValue: false));

            // PlayerPrefsV2.SetStringEncrypted and PlayerPrefsV2.GetStringDecrypted example:
            PlayerPrefsV2.SetStringEncrypted("mySecureString", "some text to encrypt", password: "myPassword123");
            var decryptedAgain = PlayerPrefsV2.GetStringDecrypted("mySecureString", null, password: "myPassword123");
            Assert.AreEqual("some text to encrypt", decryptedAgain);

            // PlayerPrefsV2.SetObject and PlayerPrefsV2.GetObject example (uses JSON internally):
            MyClass1 myObjectToSave = new MyClass1() { myString = "Im a string", myInt = 123 };
            PlayerPrefsV2.SetObject("myObject1", myObjectToSave);
            MyClass1 objLoadedAgain = PlayerPrefsV2.GetObject<MyClass1>("myObject1", defaultValue: null);
            Assert.AreEqual(myObjectToSave.myInt, objLoadedAgain.myInt);

        }

        private class MyClass1 {
            public string myString;
            public int myInt;
        }

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
            var myObj = new MyClass1() { myString = "Im a string", myInt = 123 };

            Assert.AreEqual(null, PlayerPrefsV2.GetObject<MyClass1>(key, null));
            PlayerPrefsV2.SetObject(key, myObj);
            Assert.AreEqual(myObj.myString, PlayerPrefsV2.GetObject<MyClass1>(key, null).myString);
            Assert.AreEqual(myObj.myInt, PlayerPrefsV2.GetObject<MyClass1>(key, null).myInt);
            PlayerPrefsV2.DeleteKey(key);
        }

    }

}
