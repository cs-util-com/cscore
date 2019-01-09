using System;
using com.csutil.injection;
using Xunit;

namespace com.csutil.tests {
    public class InjectionTests : DefaultTest {

        /// <summary> 
        /// The global static injector IoC.inject should not be used since the test
        /// System will execute many tests in parallel and other tests might change
        /// this global injector randomly
        /// </summary>
        private Injector GetInjectorForTest() { return Injector.newInjector(new EventBus()); }

        [Fact]
        public void TestSingleton1() {

            var IoC_inject = GetInjectorForTest();

            Assert.Null(IoC_inject.Get<MyClass1>(this));

            var singleton = new MySubClass1();
            IoC_inject.SetSingleton<MyClass1, MySubClass1>(singleton);

            var refA = IoC_inject.Get<MyClass1>(this);
            Assert.Equal(singleton, refA);
            var refB = IoC_inject.Get<MyClass1>(this);
            Assert.Equal(singleton, refB);
        }

        [Fact]
        public void TestSingleton2() {
            var IoC_inject = GetInjectorForTest();
            IoC_inject.SetSingleton<MyClass1, MySubClass1>(new MySubClass1());
            Assert.Throws<Singleton.MultipleProvidersException>(() => {
                IoC_inject.SetSingleton<MyClass1, MySubClass2>(new MySubClass2());
            });
        }

        [Fact]
        public void TestSingleton3() {
            var IoC_inject = GetInjectorForTest();
            var refA = IoC_inject.GetOrAddSingleton<MyClass1>(this);
            Assert.NotNull(refA);
            Assert.True(refA is MyClass1);
            var refB = IoC_inject.GetOrAddSingleton<MyClass1>(this);
            Assert.True(Object.ReferenceEquals(refA, refB));
        }

        [Fact]
        public void TestUnsubscribe1() {
            var IoC_inject = GetInjectorForTest();
            var injectorRef = IoC_inject.SetSingleton<MyClass1, MySubClass1>(new MySubClass1());
            Assert.True(IoC_inject.UnregisterInjector<MyClass1>(injectorRef));
            Assert.False(IoC_inject.HasInjectorRegistered<MyClass1>());

            var injector = IoC_inject.SetSingleton<MyClass1, MySubClass1>(new MySubClass1());
            Assert.True(IoC_inject.UnregisterInjector<MyClass1>(injector));
            Assert.False(IoC_inject.HasInjectorRegistered<MyClass1>());
        }

        [Fact]
        public void TestCreateIfNull() {
            var IoC_inject = GetInjectorForTest();
            { // setup an injector that will answer all requests for strings:
                string stringThatWillLazyInit = null; // lazy initialize when createIfNull=true is called the first time
                var injector1 = new object();
                IoC_inject.RegisterInjector<string>(injector1, (caller, createIfNull) => {
                    Assert.Equal(this, caller);
                    if (createIfNull) { stringThatWillLazyInit = "I am not null anymore"; }
                    return stringThatWillLazyInit;
                });
            }
            Assert.Null(IoC_inject.Get<string>(this, false));
            Assert.NotNull(IoC_inject.Get<string>(this, true));
            // now the string is initialized, it will not return null anymore for createIfNull=false
            Assert.NotNull(IoC_inject.Get<string>(this, false));
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
        public void TestIsCastableTo() {
            var IoC_inject = GetInjectorForTest();
            Assert.True(typeof(MySubClass1).IsCastableTo(typeof(MyClass1)));
            Assert.True(typeof(MyClass1).IsCastableTo(typeof(MyClass1)));
            Assert.False(typeof(MyClass1).IsCastableTo(typeof(MySubClass1)));
        }

        private class MyClass1 { }
        private class MySubClass1 : MyClass1 { }
        private class MySubClass2 : MyClass1 { }

    }
}