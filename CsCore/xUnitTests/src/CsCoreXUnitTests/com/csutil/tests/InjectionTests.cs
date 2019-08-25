using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.injection;
using Xunit;

namespace com.csutil.tests {

    public class InjectionTests {

        public InjectionTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        /// <summary> 
        /// The global static injector IoC.inject should not be used since the test
        /// System will execute many tests in parallel and other tests might change
        /// this global injector randomly
        /// </summary>
        private Injector GetInjectorForTest() { return Injector.newInjector(new EventBus()); }

        [Fact]
        public void ExampleUsage1() {
            // The default injector can be accessed via IoC.inject
            Injector injector = GetInjectorForTest();

            // Requesting an instance of MyClass1 will fail because no injector registered yet to handle requests for the MyClass1 type:
            Assert.Null(injector.Get<MyClass1>(this));

            // Setup an injector that will always return the same instance for MyClass1 when IoC.inject.Get<MyClass1>() is called:
            MySubClass1 myClass1Singleton = new MySubClass1();
            injector.GetOrAddSingleton<MyClass1>(this, () => myClass1Singleton);

            // Internally .SetSingleton() will register an injector for the class like this:
            injector.RegisterInjector<MyClass1>(new object(), (caller, createIfNull) => {
                // Whenever injector.Get is called the injector always returns the same instance:
                return myClass1Singleton; // Here the singleton could be lazy loaded
            });

            // Now calling IoC.inject.Get<MyClass1>() will always result in the same instance:
            MyClass1 myClass1 = injector.Get<MyClass1>(this);
            Assert.Same(myClass1Singleton, myClass1); // Its the same object reference
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
        public void ExampleUsage3_LazyInit() {
            var IoC_inject = GetInjectorForTest();

            { // Setup an injector1 that will answer all requests for class type string:

                // A string that will lazy initialize when createIfNull=true is called the first time
                MyClass1 objectThatWillLazyInit = null;
                var injector1 = new object();
                IoC_inject.RegisterInjector<MyClass1>(injector1, (caller, createIfNull) => {
                    // The caller passes itself in IoC.inject.Get(..) so that the injector can 
                    // react different for different callers
                    Assert.Equal(this, caller);
                    // If createIfNull was true lazy init the object:
                    if (createIfNull) { objectThatWillLazyInit = new MyClass1(); }
                    return objectThatWillLazyInit;
                });
            }

            // Calling IoC.inject.Get(..) with createIfNull false will not cause injector1 to init the string 
            Assert.Null(IoC_inject.Get<MyClass1>(this, createIfNull: false));
            // If createIfNull=true is passed the string will be initialized:
            Assert.NotNull(IoC_inject.Get<MyClass1>(this, createIfNull: true));
            // Now the string is initialized, it will not return null anymore even when createIfNull=false is passed
            Assert.NotNull(IoC_inject.Get<MyClass1>(this, createIfNull: false));
        }

        [Fact]
        public void TestSingletons2() {
            var IoC_inject = GetInjectorForTest();
            var injector = new object();

            // Set an injector1 for MyClass1:
            MyClass1 ref1 = IoC_inject.GetOrAddSingleton<MyClass1>(injector, () => new MySubClass1());
            Assert.Same(ref1, IoC_inject.Get<MyClass1>(this));
            Assert.Equal(typeof(MySubClass1), IoC_inject.Get<MyClass1>(this).GetType());

            // Try to register an additional injector for the same class which should fail:
            Assert.Throws<Singleton.MultipleProvidersException>(() => {
                IoC_inject.SetSingleton<MyClass1>(new MySubClass2());
            });
            // The first provider is still active and provides the same singleton instance as before:
            Assert.Equal(typeof(MySubClass1), IoC_inject.Get<MyClass1>(this).GetType());

            // Remove the initial singleton to replace it with a new one:
            Assert.True(IoC_inject.UnregisterInjector<MyClass1>(injector));
            // Test that GetOrAddSingleton is not allowed to return null as an instance:
            Assert.Throws<ArgumentNullException>(() => { IoC_inject.GetOrAddSingleton<MyClass1>(injector, () => null); });
            // Register an injector that instantly creates a MySubClass2 instance:
            MyClass1 ref2 = IoC_inject.GetOrAddSingleton<MyClass1>(injector, () => new MySubClass2());
            Assert.Equal(typeof(MySubClass2), IoC_inject.Get<MyClass1>(this).GetType());
            Assert.NotEqual(ref1, ref2);

            // Override the current singleton:
            IoC_inject.SetSingleton<MyClass1>(injector, new MySubClass1(), overrideExisting: true);
            Assert.Equal(typeof(MySubClass1), IoC_inject.Get<MyClass1>(this).GetType());

            // Check that the initial injector1 was overwritten by injector2:
            Assert.True(IoC_inject.UnregisterInjector<MyClass1>(injector));
            Assert.Null(IoC_inject.Get<MyClass1>(this));
        }

        [Fact]
        public void TestUnsubscribe1() {
            var IoC_inject = GetInjectorForTest();
            var injector = new object();

            // Setup a normal singleton injector1:
            MyClass1 ref1 = IoC_inject.GetOrAddSingleton<MyClass1>(injector, () => new MySubClass1());
            Assert.NotNull(IoC_inject.Get<MyClass1>(this));
            Assert.Same(ref1, IoC_inject.Get<MyClass1>(this));

            // Now unregister the injector1 again:
            Assert.True(IoC_inject.UnregisterInjector<MyClass1>(injector));
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
        public void TestMultipleInjectors2() {
            var IoC_inject = GetInjectorForTest();

            Assert.False(IoC_inject.HasInjectorRegistered<MyClass1>());
            IoC_inject.RegisterInjector<MyClass1>(new object(), (caller, createIfNull) => {
                Log.d("Injector with MySubClass1 was called");
                return new MySubClass1();
            });
            Assert.True(IoC_inject.HasInjectorRegistered<MyClass1>());

            var secondInjectorWasUsed = false;
            IoC_inject.RegisterInjector<MyClass1>(new object(), (caller, createIfNull) => {
                Log.d("Injector with MySubClass2 was called");
                secondInjectorWasUsed = true;
                return new MySubClass2();
            });
            Assert.False(secondInjectorWasUsed);
            Assert.NotNull(IoC_inject.Get<MyClass1>(this));
            Assert.False(secondInjectorWasUsed);
            Assert.True(IoC_inject.Get<MyClass1>(this) is MySubClass1);
            Assert.False(secondInjectorWasUsed);
            var bothResults = IoC_inject.GetAll<MyClass1>(this);
            Assert.True(bothResults.First() is MySubClass1);
            Assert.False(secondInjectorWasUsed); // before accessing .Last the injection was not yet triggered
            Assert.True(bothResults.Last() is MySubClass2);
            Assert.True(secondInjectorWasUsed); // after accessing .Last the injection was triggered
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
        private class MyUserClass1 { }

        [Fact]
        public void TestTemporaryContext1() {
            var IoC_inject = GetInjectorForTest();
            Assert.Null(IoC_inject.Get<MyClass1>(this));
            for (int i = 0; i < 100; i++) {
                TaskV2.Run(() => {
                    var myContextInstance1 = new MyClass1();
                    IoC_inject.DoWithTempContext<MyClass1>(myContextInstance1, () => {
                        Assert.Equal(myContextInstance1, IoC_inject.Get<MyClass1>(this));
                    });
                    // when the temporary context is gone requesting an injection returns null again:
                    Assert.Null(IoC_inject.Get<MyClass1>(this));

                    var myContextInstance2 = new MyClass1();
                    var testUser = new MyUserClass1();
                    IoC_inject.DoWithTempContext<MyClass1>(myContextInstance2, () => {
                        IoC_inject.DoWithTempContext<MyUserClass1>(testUser, () => {
                            Assert.Equal(myContextInstance2, IoC_inject.Get<MyClass1>(this));
                            Assert.Equal(testUser, IoC_inject.Get<MyUserClass1>(this));
                        });
                    });
                    // when the temporary context is gone requesting an injection returns null again:
                    Assert.Null(IoC_inject.Get<MyClass1>(this));
                    Assert.Null(IoC_inject.Get<MyUserClass1>(this));
                });
            }
        }

        [Fact]
        public void TestOverrideOldSubscriber() {

            Injector injector = GetInjectorForTest();
            var injectionHandler = new object();
            // Register an injector that will be replaced by a second one:
            injector.RegisterInjector<MyClass1>(injectionHandler, (caller, createIfNull) => { throw Log.e("Should be replaced"); });
            // Replace it:
            MySubClass1 myClass1Singleton = new MySubClass1();
            injector.RegisterInjector<MyClass1>(injectionHandler, (caller, createIfNull) => myClass1Singleton);
            Assert.Same(myClass1Singleton, injector.Get<MyClass1>(this));

        }

    }
}