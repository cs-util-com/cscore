using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    public class DataStoreExample6_CompositeStores {

        public DataStoreExample6_CompositeStores(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1_MultipleStores() {

            var m1 = new Model1() { a = "a" };
            var m2 = new Model2() { b = "b" };
            var compositeStore = new CompositeDataStore<Model1, Model2>(Model1Reducer, null, m1, Model2Reducer, m2, null);

            // This composite store can be used both as a store to access Model1 and Model2:
            IDataStore<Model1> model1Store = compositeStore;
            IDataStore<Model2> model2Store = compositeStore;

            Model1 state1 = model1Store.GetState();
            Assert.Equal("a", state1.a);
            Model2 state2 = model2Store.GetState();
            Assert.Equal("b", state2.b);

            int counterAllStores = 0;
            model2Store.onStateChanged += () => { counterAllStores++; };
            Assert.Equal(0, counterAllStores);

            var counter1 = 0;
            model1Store.AddStateChangeListener(m => m, (model1) => {
                Assert.Equal("a2", model1.a);
                counter1++;
            }, false);

            var counter2 = 0;
            model2Store.AddStateChangeListener(m => m, (model2) => {
                Assert.Equal("b2", model2.b);
                counter2++;
            }, false);

            model2Store.Dispatch(new ActionChangeA() { newA = "a2" });
            Assert.Equal("a2", model1Store.GetState().a);
            Assert.Equal(1, counterAllStores);
            Assert.Equal(1, counter1);
            Assert.Equal(0, counter2);

            model2Store.Dispatch(new ActionChangeB() { newB = "b2" });
            Assert.Equal("b2", model2Store.GetState().b);
            Assert.Equal(2, counterAllStores);
            Assert.Equal(1, counter1);
            Assert.Equal(1, counter2);
        }

        [Fact]
        public void ExampleUsage2_MultipleStores() {

            // Normally using the composite store pattern only requires to
            // combine 2 stores but more is possible as this example here shows:
            var compositeStore = NewChainedCompositeStore();

            // All stores could still be accessed:
            IDataStore<Model1> model1Store = compositeStore.GetInnerStore() as IDataStore<Model1>;
            IDataStore<Model2> model2Store = compositeStore;
            IDataStore<Model3> model3Store = compositeStore;

            Model1 state1 = model1Store.GetState();
            Assert.Equal("a", state1.a);
            Model2 state2 = model2Store.GetState();
            Assert.Equal("b", state2.b);
            Model3 state3 = model3Store.GetState();
            Assert.Equal("c", state3.c);

            int counterAllStores = 0;
            model2Store.onStateChanged += () => { counterAllStores++; };
            Assert.Equal(0, counterAllStores);

            var counter1 = 0;
            model1Store.AddStateChangeListener(m => m, (model1) => {
                Assert.Equal("a2", model1.a);
                counter1++;
            }, false);

            var counter2 = 0;
            model2Store.AddStateChangeListener(m => m, (model2) => {
                Assert.Equal("b2", model2.b);
                counter2++;
            }, false);

            model2Store.Dispatch(new ActionChangeA() { newA = "a2" });
            Assert.Equal("a2", model1Store.GetState().a);
            Assert.Equal(1, counterAllStores);
            Assert.Equal(1, counter1);
            Assert.Equal(0, counter2);

            model2Store.Dispatch(new ActionChangeB() { newB = "b2" });
            Assert.Equal("b2", model2Store.GetState().b);
            Assert.Equal(2, counterAllStores);
            Assert.Equal(1, counter1);
            Assert.Equal(1, counter2);

            model2Store.Dispatch(new ActionChangeC() { newC = "c2" });
            Assert.Equal("c2", model3Store.GetState().c);
            Assert.Equal(3, counterAllStores);
            Assert.Equal(1, counter1);
            Assert.Equal(1, counter2);

        }

        private CompositeDataStore<Model3, Model2> NewChainedCompositeStore() {
            var m1 = new Model1() { a = "a" };
            var m2 = new Model2() { b = "b" };
            var m3 = new Model3() { c = "c" };
            var innerCompStore = new CompositeDataStore<Model1, Model2>(Model1Reducer, null, m1, Model2Reducer, m2, null);
            return new CompositeDataStore<Model3, Model2>(Model3Reducer, null, m3, innerCompStore);
        }

        private class Model1 {
            public string a;
        }

        private class Model2 {
            public string b;
        }

        private class Model3 {
            public string c;
        }

        public class ActionChangeA {
            public string newA;
        }

        public class ActionChangeB {
            public string newB;
        }

        public class ActionChangeC {
            public string newC;
        }

        private Model1 Model1Reducer(Model1 previousstate, object action) {
            if (action is ActionChangeA a) { return new Model1() { a = a.newA }; }
            return previousstate;
        }

        private Model2 Model2Reducer(Model2 previousstate, object action) {
            if (action is ActionChangeB a) { return new Model2() { b = a.newB }; }
            return previousstate;
        }

        private Model3 Model3Reducer(Model3 previousstate, object action) {
            if (action is ActionChangeC a) { return new Model3() { c = a.newC }; }
            return previousstate;
        }

    }

}