using NUnit.Framework;
using UnityEngine;

namespace com.csutil.tests.injection {

    class InjectionSetupTests {

        [Test]
        public void Test__UnitySetup_InvokeAfterUnitySetupDone() {
            // See MyExampleInjectionSetup below why instance will not be null:
            var instance = IoC.inject.Get<MyExampleClass1ToInject>(this);
            Assert.NotNull(instance);
            Assert.AreEqual("Handled by MyExampleInjectionSetup", instance.myString);
        }

    }

    class MyExampleClass1ToInject {
        public string myString;
    }

    /// <summary> Automatically invoked by Unity because of the [RuntimeInitializeOnLoadMethod] annotation </summary>
    class MyExampleInjectionSetup {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void SetupAllMyInjections() {
            // In here the injection setup can be done AFTER the generic UnitySetup finished its own setup:
            UnitySetup.InvokeAfterUnitySetupDone(() => {
                var injector1 = new object();
                IoC.inject.RegisterInjector<MyExampleClass1ToInject>(injector1, (requester, createIfNull) => {
                    return new MyExampleClass1ToInject() { myString = "Handled by MyExampleInjectionSetup" };
                });
                // .. here more injectors would be registered for other classes .. //
            });

        }

    }

}
