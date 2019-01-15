using System;
using com.csutil.injection;
using Xunit;

namespace com.csutil.tests {

    public class InjectionTests {

        /// <summary> 
        /// The global static injector IoC.inject should not be used since the test
        /// System will execute many tests in parallel and other tests might change
        /// this global injector randomly
        /// </summary>
        private Injector GetInjectorForTest() { return Injector.newInjector(new EventBus()); }

        [Fact]
        public void ExampleUsage1() {
            // The default injector can be accessed via IoC.inject
            Injector IoC_inject = GetInjectorForTest();

            // Requesting an instance of MyClass1 will fail because
            // no injector registered yet to handle requests for the MyClass1 type:
            Assert.Null(IoC_inject.Get<MyClass1>(this));

            // Setup an injector that will always return the same instance for MyClass1
            // when IoC.inject.Get<MyClass1>() is called:
            var singletonInstance = new MySubClass1();
            IoC_inject.SetSingleton<MyClass1, MySubClass1>(singletonInstance);

            // Now calling IoC.inject.Get<MyClass1>() will always result in the same instance:
            var refA = IoC_inject.Get<MyClass1>(this);
            Assert.Equal(singletonInstance, refA);
            var refB = IoC_inject.Get<MyClass1>(this);
            Assert.Equal(singletonInstance, refB);
        }

        [Fact]
        public void ExampleUsage2() {
            var IoC_inject = GetInjectorForTest();

            // GetOrAddSingleton will automatically init a singleton instance and setup a provider:
            var singletonRef1 = IoC_inject.GetOrAddSingleton<MyClass1>(this);
            Assert.NotNull(singletonRef1);
            Assert.True(singletonRef1 is MyClass1);

            // When GetOrAddSingleton is called a second time the same instance is returned as before:
            var singletonRef2 = IoC_inject.GetOrAddSingleton<MyClass1>(this);

            // The returned references must be equal:           
            Assert.True(Object.ReferenceEquals(singletonRef1, singletonRef2));
            // The same singleton instance can be accessed via the normal IoC.inject.Get(..):
            Assert.True(Object.ReferenceEquals(singletonRef1, IoC_inject.Get<MyClass1>(this)));

            // The singleton provider can be removed again:
            Assert.True(IoC_inject.RemoveAllInjectorsFor<MyClass1>());
            Assert.Null(IoC_inject.Get<MyClass1>(this));

            // If there is already a provider set for MyClass1 GetOrAddSingleton will not create a singleton provider:
            var singletonInstance = new MySubClass1();
            IoC_inject.RegisterInjector<MyClass1>(new object(), (_, createIfNull) => {
                return createIfNull ? singletonInstance : null;
            });
            Assert.Same(singletonInstance, IoC_inject.Get<MyClass1>(this));
            Assert.Same(singletonInstance, IoC_inject.GetOrAddSingleton<MyClass1>(this));
        }

        [Fact]
        public void ExampleUsage3() {
            var IoC_inject = GetInjectorForTest();

            { // Setup an injector1 that will answer all requests for class type string:

                // A string that will lazy initialize when createIfNull=true is called the first time
                string stringThatWillLazyInit = null;
                var injector1 = new object();
                IoC_inject.RegisterInjector<string>(injector1, (caller, createIfNull) => {
                    // The caller passes itself in IoC.inject.Get(..) so that the injector can 
                    // react different for different callers
                    Assert.Equal(this, caller);
                    // If createIfNull was true lazy init the string:
                    if (createIfNull) { stringThatWillLazyInit = "I am not null anymore"; }
                    return stringThatWillLazyInit;
                });
            }

            // Calling IoC.inject.Get(..) with createIfNull false will not cause injector1 to init the string 
            Assert.Null(IoC_inject.Get<string>(this, createIfNull: false));
            // If createIfNull=true is passed the string will be initialized:
            Assert.NotNull(IoC_inject.Get<string>(this, createIfNull: true));
            // Now the string is initialized, it will not return null anymore even when createIfNull=false is passed
            Assert.NotNull(IoC_inject.Get<string>(this, createIfNull: false));
        }

        [Fact]
        public void TestSingletons2() {
            var IoC_inject = GetInjectorForTest();

            // Set an injector1 for MyClass1:
            var injector1 = IoC_inject.SetSingleton<MyClass1, MySubClass1>(new MySubClass1());
            Assert.Equal(typeof(MySubClass1), IoC_inject.Get<MyClass1>(null).GetType());

            // Try to register an additional injector for the same class which should fail:
            Assert.Throws<Singleton.MultipleProvidersException>(() => {
                IoC_inject.SetSingleton<MyClass1, MySubClass2>(new MySubClass2());
            });
            // The first provider is still active and provides the same singleton instance as before:
            Assert.Equal(typeof(MySubClass1), IoC_inject.Get<MyClass1>(null).GetType());

            // Force overriding the initial injector1:
            var injector2 = IoC_inject.SetSingleton<MyClass1, MySubClass2>(new MySubClass2(), true);
            Assert.Equal(typeof(MySubClass2), IoC_inject.Get<MyClass1>(null).GetType());

            // Check that the initial injector1 was overwritten by injector2:
            Assert.False(IoC_inject.UnregisterInjector<MyClass1>(injector1));
            Assert.True(IoC_inject.UnregisterInjector<MyClass1>(injector2));
            Assert.Null(IoC_inject.Get<MyClass1>(null));
        }

        [Fact]
        public void TestUnsubscribe1() {
            var IoC_inject = GetInjectorForTest();

            // Setup a normal singleton injector1:
            var injector1 = IoC_inject.SetSingleton<MyClass1, MySubClass1>(new MySubClass1());
            Assert.NotNull(IoC_inject.Get<MyClass1>(this));

            // Now unregister the injector1 again:
            Assert.True(IoC_inject.UnregisterInjector<MyClass1>(injector1));
            Assert.False(IoC_inject.HasInjectorRegistered<MyClass1>());
            Assert.Null(IoC_inject.Get<MyClass1>(this));
        }

        [Fact]
        public void TestMultipleInjectors() {
            var IoC_inject = GetInjectorForTest();
            { // the first injector will only react if the caller is caller 1
                var injector1 = new object();
                IoC_inject.RegisterInjector<MyClass1>(injector1, (caller, createIfNull) => {
                    if ((string)caller == "caller 1") { return new MySubClass1(); }
                    return null;
                });
            }
            { // the second injector will only react if the caller is caller 2
                var injector2 = new object();
                IoC_inject.RegisterInjector<MyClass1>(injector2, (caller, createIfNull) => {
                    if ((string)caller == "caller 2") { return new MySubClass2(); }
                    return null;
                });
            }
            Assert.True(IoC_inject.Get<MyClass1>("caller 1") is MySubClass1);
            Assert.True(IoC_inject.Get<MyClass1>("caller 2") is MySubClass2);
            // both injectors don't react if any other caller asks for an instance:
            Assert.Null(IoC_inject.Get<MyClass1>("caller 3"));
        }

        [Fact]
        public void TestRemoveAllInjectors() {
            var IoC_inject = GetInjectorForTest();

            var injector1 = new object();
            Assert.False(IoC_inject.RemoveAllInjectorsFor<string>());
            IoC_inject.RegisterInjector<string>(injector1, (c, _) => "");
            Assert.True(IoC_inject.RemoveAllInjectorsFor<string>());
            Assert.False(IoC_inject.RemoveAllInjectorsFor<string>());
        }

        private class MyClass1 { }
        private class MySubClass1 : MyClass1 { }
        private class MySubClass2 : MyClass1 { }

    }
}