using System;
using com.csutil.injection;
using Xunit;

namespace com.csutil.tests {
    public class InjectionTests : IDisposable {

        public InjectionTests() { // Setup before each test:
            // Reset EventBus and Injection:
            EventBus.instance = new EventBus();
            IoC.inject = new Injector();
        }

        public void Dispose() { // TearDown after each test:
        }

        [Fact]
        public void TestSingleton1() {
            Assert.Null(IoC.inject.Get<MyClass1>(this));

            var singleton = new MySubClass1();
            IoC.inject.SetSingleton<MyClass1, MySubClass1>(singleton);

            var refA = IoC.inject.Get<MyClass1>(this);
            Assert.Equal(singleton, refA);
            var refB = IoC.inject.Get<MyClass1>(this);
            Assert.Equal(singleton, refB);
        }

        [Fact]
        public void TestSingleton2() {
            IoC.inject.SetSingleton<MyClass1, MySubClass1>(new MySubClass1());
            Assert.Throws<Singleton.MultipleProvidersException>(() => {
                IoC.inject.SetSingleton<MyClass1, MySubClass2>(new MySubClass2());
            });
        }

        [Fact]
        public void TestSingleton3() {
            var refA = IoC.inject.GetOrAddSingleton<MyClass1>(this);
            Assert.NotNull(refA);
            Assert.True(refA is MyClass1);
            var refB = IoC.inject.GetOrAddSingleton<MyClass1>(this);
            Assert.Equal(refA, refB);
        }

        [Fact]
        public void TestUnsubscribe1() {
            var injectorRef = IoC.inject.SetSingleton<MyClass1, MySubClass1>(new MySubClass1());
            Assert.True(IoC.inject.UnregisterInjector<MyClass1>(injectorRef));
            Assert.False(IoC.inject.HasInjectorRegistered<MyClass1>());

            IoC.inject.SetSingleton<MyClass1, MySubClass1>(new MySubClass1());
            Assert.True(IoC.inject.UnregisterAllInjectors<MyClass1>());
            Assert.False(IoC.inject.HasInjectorRegistered<MyClass1>());
        }

        [Fact]
        public void TestCreateIfNull() {
            { // setup an injector that will answer all requests for strings:
                string stringThatWillLazyInit = null; // lazy initialize when createIfNull=true is called the first time
                var injector1 = new object();
                IoC.inject.RegisterInjector<string>(injector1, (caller, createIfNull) => {
                    Assert.Equal(this, caller);
                    if (createIfNull) { stringThatWillLazyInit = "I am not null anymore"; }
                    return stringThatWillLazyInit;
                });
            }
            Assert.Null(IoC.inject.Get<string>(this, false));
            Assert.NotNull(IoC.inject.Get<string>(this, true));
            // now the string is initialized, it will not return null anymore for createIfNull=false
            Assert.NotNull(IoC.inject.Get<string>(this, false));
        }

        [Fact]
        public void TestMultipleInjectors() {
            { // the first injector will only react if the caller is caller 1
                var injector1 = new object();
                IoC.inject.RegisterInjector<MyClass1>(injector1, (caller, createIfNull) => {
                    if ((string)caller == "caller 1") { return new MySubClass1(); }
                    return null;
                });
            }
            { // the second injector will only react if the caller is caller 2
                var injector2 = new object();
                IoC.inject.RegisterInjector<MyClass1>(injector2, (caller, createIfNull) => {
                    if ((string)caller == "caller 2") { return new MySubClass2(); }
                    return null;
                });
            }
            Assert.True(IoC.inject.Get<MyClass1>("caller 1") is MySubClass1);
            Assert.True(IoC.inject.Get<MyClass1>("caller 2") is MySubClass2);
            // both injectors don't react if any other caller asks for an instance:
            Assert.Null(IoC.inject.Get<MyClass1>("caller 3"));
        }

        private class MyClass1 { }
        private class MySubClass1 : MyClass1 { }
        private class MySubClass2 : MyClass1 { }

    }
}