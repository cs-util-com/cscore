using com.csutil.tests;
using NUnit.Framework;
using System;
using UnityEngine;

namespace com.csutil.tests.injection {

    public class ComponentInjectionTests {

        /// <summary>
        /// Calling GetOrAddComponentSingleton will create a singleton. The parent 
        /// gameobject of this singleton will be created together with it in
        /// the scene. The location of the singleton will be:
        /// 
        /// "Singletons" GameObject -> "MyExampleMono1" GameObject -> MyExampleMono1
        /// 
        /// This way all created singletons will be created and grouped together in the 
        /// "Singletons" GameObject and accessible like any other MonoBehaviour as well.
        /// </summary>
        [Test]
        public void ExampleUsage1() {
            // Initially there is no MonoBehaviour registered in the system:
            Assert.IsNull(IoC.inject.Get<MyExampleMono1>(this));

            // Calling GetOrAddComponentSingleton will create a singleton:
            MyExampleMono1 x1 = IoC.inject.GetOrAddComponentSingleton<MyExampleMono1>(this);

            // Calling GetOrAddComponentSingleton again now returns the singleton:
            MyExampleMono1 x2 = IoC.inject.GetOrAddComponentSingleton<MyExampleMono1>(this);
            Assert.AreSame(x1, x2); // Both references point to the same object

            // Calling the default IoC.inject.Get will also return the same singleton:
            MyExampleMono1 x3 = IoC.inject.Get<MyExampleMono1>(this);
            Assert.AreSame(x1, x3); // Both references point to the same object
        }

        [Test]
        public void TestGetOrAddComponentSingleton() {
            IoC.inject.RemoveAllInjectorsFor<MyExampleMono1>();
            AssertV2.Throws<Exception>(() => {
                // It should not be possible to create a mono via default constructor:
                IoC.inject.GetOrAddSingleton<MyExampleMono1>(this);
            });
            {
                var singletonsName = "SingletonsMaster1";
                var x1 = IoC.inject.GetOrAddComponentSingleton<MyExampleMono1>(this, singletonsName);
                Assert.NotNull(x1);
                var go = x1.gameObject;
                Assert.NotNull(go);
                Assert.AreEqual(singletonsName, go.GetParent().name);

                var x2 = IoC.inject.Get<MyExampleMono1>(this);
                Assert.AreEqual(x1, x2);
                Assert.AreEqual(x1.gameObject, x2.gameObject);
            }
        }

        [Test]
        public void TestGetOrAddScriptableObject() {
            string pathToSoInstance1 = "MyExampleScriptableObject_Instance1";
            string someStringValue = "some string 123";
            { // 
                var i = ResourcesV2.LoadScriptableObjectInstance<MyExampleScriptableObject>(pathToSoInstance1);
                Assert.IsNotNull(i);
                i.myString1 = someStringValue; 
            }
            {
                // Load a ScriptableObject instance and set it as the singleton:
                var instance1 = ResourcesV2.LoadScriptableObjectInstance<MyExampleScriptableObject>(pathToSoInstance1);
                IoC.inject.SetSingleton(instance1);

                // Loading ScriptableObject instances multiple times via ResourcesV2.LoadScriptableObjectInstance will 
                // result in the same instance each time, so the myString1 will be modified now:
                Assert.AreEqual(someStringValue, instance1.myString1);

                // Now that the singleton is set this instance is always returned for the ScriptableObject class:
                var instance1ViaIoC = IoC.inject.Get<MyExampleScriptableObject>(this);
                Assert.AreSame(instance1, instance1ViaIoC);
                Assert.AreEqual(someStringValue, instance1.myString1);
            }

        }

        private class MyClass1 { }

        [Test]
        public void TestSingletonInjection() {
            // TODO this case is already fully covered by the pure C# singleton tests!
            var singletonInstance = new MyClass1();
            IoC.inject.SetSingleton(singletonInstance);

            var a = IoC.inject.Get<MyClass1>(this);
            var b = IoC.inject.Get<MyClass1>(this);
            Assert.AreEqual(a, b);
            Assert.AreEqual(singletonInstance, a);
        }

    }

}
