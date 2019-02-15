using com.csutil.injection;
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

            { // Calling GetOrAddComponentSingleton again now returns the singleton:
                MyExampleMono1 x2 = IoC.inject.GetOrAddComponentSingleton<MyExampleMono1>(this);
                Assert.AreSame(x1, x2); // Both references point to the same object
            }

            { // Calling the default IoC.inject.Get will also return the same singleton:
                MyExampleMono1 x2 = IoC.inject.Get<MyExampleMono1>(this);
                Assert.AreSame(x1, x2); // Both references point to the same object
            }

        }

        [Test]
        public void ExampleUsage2() {

            // Load a ScriptableObject instance and set it as the singleton:
            var path = "MyExampleScriptableObject_Instance1.asset";
            MyExampleScriptableObject x1 = ResourcesV2.LoadScriptableObjectInstance<MyExampleScriptableObject>(path);
            IoC.inject.SetSingleton(x1);

            // Now that the singleton is set this instance is always returned for the ScriptableObject class:
            MyExampleScriptableObject x2 = IoC.inject.Get<MyExampleScriptableObject>(this);
            Assert.AreSame(x1, x2);

        }

        [Test]
        public void TestGetOrAddComponentSingleton() {
            Injector injector = GetInjectorForTesting();

            injector.RemoveAllInjectorsFor<MyExampleMono1>();
            AssertV2.Throws<Exception>(() => {
                // It should not be possible to create a mono via default constructor:
                injector.GetOrAddSingleton<MyExampleMono1>(this);
            });
            {
                var singletonsName = "SingletonsMaster1";
                var x1 = injector.GetOrAddComponentSingleton<MyExampleMono1>(this, singletonsName);
                Assert.NotNull(x1);
                var go = x1.gameObject;
                Assert.NotNull(go);
                Assert.AreEqual(singletonsName, go.GetParent().name);

                var x2 = injector.Get<MyExampleMono1>(this);
                Assert.AreEqual(x1, x2);
                Assert.AreEqual(x1.gameObject, x2.gameObject);
            }
        }

        [Test]
        public void TestScriptableObjectSingleton() {
            Injector injector = GetInjectorForTesting();

            // The path to an ScriptableObject instance .asset file in a Resources folder:
            string pathToSoInstance1 = "MyExampleScriptableObject_Instance1.asset";
            string someStringValue = "some string 123";
            { // ScriptableObject instances can be accessed via ResourcesV2.LoadScriptableObjectInstance:
                var i = ResourcesV2.LoadScriptableObjectInstance<MyExampleScriptableObject>(pathToSoInstance1);
                Assert.IsNotNull(i);
                i.myString1 = someStringValue;
            }
            {
                // Load a ScriptableObject instance and set it as the singleton:
                var instance1 = ResourcesV2.LoadScriptableObjectInstance<MyExampleScriptableObject>(pathToSoInstance1);
                injector.SetSingleton(instance1);

                // Loading ScriptableObject instances multiple times via ResourcesV2.LoadScriptableObjectInstance will 
                // result in the same instance each time, so the myString1 will be modified now:
                Assert.AreEqual(someStringValue, instance1.myString1);

                // Now that the singleton is set this instance is always returned for the ScriptableObject class:
                var instance1ViaIoC = injector.Get<MyExampleScriptableObject>(this);
                Assert.AreSame(instance1, instance1ViaIoC);
                Assert.AreEqual(someStringValue, instance1.myString1);
            }

        }

        private static Injector GetInjectorForTesting() {
            return Injector.newInjector(new EventBus());
        }

        private class MyClass1 { }

        [Test]
        public void TestSingletonInjection() {
            Injector injector = GetInjectorForTesting();

            // TODO this case is already fully covered by the pure C# singleton tests!
            var singletonInstance = new MyClass1();
            injector.SetSingleton(singletonInstance);

            var a = injector.Get<MyClass1>(this);
            var b = injector.Get<MyClass1>(this);
            Assert.AreEqual(a, b);
            Assert.AreEqual(singletonInstance, a);
        }

    }

}
