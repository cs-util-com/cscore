using com.csutil.io;
using com.csutil.keyvaluestore;
using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests.io {

    public class PlayerPrefsV2Tests {

        [Test]
        public void ExampleUsage1() {
            string key1 = "key1";
            string value1 = "value1";

            var store = PlayerPrefsV2.instance;
            store.Set(key1, value1);
            string x = store.Get(key1, "defaultValue1").Result;
            store.Remove(key1); // cleanup
            Assert.AreEqual(value1, x);
        }

        [Test]
        public void ExampleUsage2() {

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

        [UnityTest]
        public IEnumerator TestPlayerPrefsAsKeyValueStore() {
            yield return TestIKeyValueStoreImplementation(new PlayerPrefsStore()).AsCoroutine();
        }

        /// <summary> Runs typical requests on the passed store </summary>
        private static async Task TestIKeyValueStoreImplementation(IKeyValueStore store) {
            string myKey1 = "myKey1";
            var myValue1 = "myValue1";
            string myKey2 = "myKey2";
            var myValue2 = "myValue2";
            var myFallbackValue1 = "myFallbackValue1";

            // Cleanup before actual test starts:
            await store.Remove(myKey1);
            await store.Remove(myKey2);

            // test Set and Get of values:
            Assert.False(await store.ContainsKey(myKey1));
            Assert.AreEqual(myFallbackValue1, await store.Get(myKey1, myFallbackValue1));
            await store.Set(myKey1, myValue1);
            Assert.AreEqual(myValue1, await store.Get<string>(myKey1, null));
            Assert.True(await store.ContainsKey(myKey1));

            // Test replacing values:
            var oldVal = await store.Set(myKey1, myValue2);
            Assert.AreEqual(myValue1, oldVal);
            Assert.AreEqual(myValue2, await store.Get<string>(myKey1, null));

            // Test add and remove of a second key:
            Assert.False(await store.ContainsKey(myKey2));
            await store.Set(myKey2, myValue2);
            Assert.True(await store.ContainsKey(myKey2));

            await store.Remove(myKey2);
            Assert.False(await store.ContainsKey(myKey2));

        }

        [UnityTest]
        public IEnumerator TestPlayerPrefsFromBackgroundThread() {
            yield return TestPlayerPrefsFromBackgroundThreadTasks().AsCoroutine();
        }

        private async Task TestPlayerPrefsFromBackgroundThreadTasks() {
            var myKey1 = "myKey1";
            var myVal1 = "myVal1";
            var myFallback1 = "myFallback1";

            var innerStore = new ExceptionWrapperKeyValueStore(new PlayerPrefsStore());
            await innerStore.Remove(myKey1); // Cleanup prefs from previous tests
            var outerStore = new InMemoryKeyValueStore().WithFallbackStore(innerStore);

            var task = TaskRunner.instance.RunInBackground(async (cancel) => {
                cancel.ThrowIfCancellationRequested();
                Assert.IsFalse(MainThread.isMainThread);

                var innerStoreThrewAnError = false;
                // Set and Get from a background thread will throw an exception in the innerStore
                innerStore.onError = (e) => { innerStoreThrewAnError = true; };
                // So only the outerStore will be updated when calling Set and Get from the background:
                await outerStore.Set(myKey1, myVal1);
                Assert.IsTrue(innerStoreThrewAnError);
                var x = await outerStore.Get(myKey1, myFallback1);
                // The value returned by Get was cached in the outer store so it will be correct:
                Assert.AreEqual(myVal1, x);
                // Check that the Set request never reached the real pref. store:
                Assert.AreEqual(myFallback1, await innerStore.Get(myKey1, myFallback1));
            }).task;
            await task;
            Assert.IsTrue(task.IsCompleted);
            Assert.IsNull(task.Exception);

            Assert.IsTrue(MainThread.isMainThread);
            // There should not be any errors when working on the main thread so 
            // throw any errors that happen in the inner store:
            innerStore.onError = (e) => { throw e; };
            // In the main thread Set and Get will not throw errors:
            await outerStore.Set(myKey1, myVal1);
            Assert.AreEqual(myVal1, await outerStore.Get(myKey1, myFallback1));
            // Check that the Set request never reached the real pref. store:
            Assert.AreEqual(myVal1, await innerStore.Get(myKey1, myFallback1));
        }
    }

}
