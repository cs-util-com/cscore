using System;
using System.Linq;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    /// <summary>
    /// This example shows how to use the SlicedModel to create a store that contains multiple model slices.
    /// A slice is an independent model that can be added to a store and removed from it at any time.
    /// This allows to create a store that contains multiple independent models that can be accessed via the store.
    /// </summary>
    public class DataStoreExample7_StoreSlicing {

        public DataStoreExample7_StoreSlicing(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            // Create a store with 2 slices:
            var slices = new SlicedModel(new[] {
                SlicedModel.Slice.New(new Model1(), Model1Reducer),
                SlicedModel.Slice.New(new Model2(), Model2Reducer),
            });
            IDataStore<SlicedModel> store = new DataStore<SlicedModel>(SlicedModel.Reducer, slices, Middlewares.NewLoggingMiddleware<SlicedModel>());

            // Adding slices to an already created store is possible as well:
            store.AddSlice(new Model3(), Model3Reducer);

            IDataStore<Model1> store1 = store.GetStore<Model1>();
            IDataStore<Model2> store2 = store.GetStore<Model2>();
            IDataStore<Model3> store3 = store.GetStore<Model3>();

            { // Initially all slices are empty:
                Assert.Null(store1.GetState().a);
                Assert.Null(store2.GetState().b);
                Assert.Null(store3.GetState().c);
                // dispatching to store or store1 has the same effect:
                store1.Dispatch(new ActionChangeA() { newA = "a" });
                Assert.Equal("a", store1.GetState().a);
                Assert.Null(store2.GetState().b);
                Assert.Null(store3.GetState().c);
            }
            {
                var slice1 = store.GetState().GetSlice<Model1>();
                var slice2 = store.GetState().GetSlice<Model2>();
                Assert.Equal("a", slice1.a);
                store1.Dispatch(new ActionChangeB() { newB = "b" });
                Assert.Same(slice1, store.GetState().GetSlice<Model1>());
                Assert.NotSame(slice2, store.GetState().GetSlice<Model2>());
                Assert.Equal("a", store1.GetState().a);
                Assert.Equal("b", store2.GetState().b);
                Assert.Null(store3.GetState().c);
            }
            {
                store3.Dispatch(new ActionChangeC() { newC = "c" });
                Assert.Equal("a", store1.GetState().a);
                Assert.Equal("b", store2.GetState().b);
                Assert.Equal("c", store3.GetState().c);
                store1.Dispatch(new ActionChangeA() { newA = "a2" });
                Assert.Equal("a2", store1.GetState().a);
            }
            { // GetState can return the slice of the store:
                var model2 = store.GetState<Model2>();
                Assert.Equal("b", model2.GetState().b);
                // GetSubState can automatically access the correct slice of the store:
                var model3c = store.GetSubState((Model3 x) => x.c);
                Assert.Equal("c", model3c.GetState());
                var cChangedCounter = 0;
                model3c.onStateChanged += () => { cChangedCounter++; };
                Assert.Equal(0, cChangedCounter);
                store.Dispatch(new ActionChangeC() { newC = "c2" });
                Assert.Equal("c2", model3c.GetState());
                Assert.Equal(1, cChangedCounter);
            }

            // Removing slices from the store is possible as well:
            store.RemoveSlice(store.GetState().Slices.Last());
            Assert.Equal("a2", store1.GetState().a);
            Assert.Equal("b", store2.GetState().b);
            // The removed slice can not be accessed anymore:
            Assert.Throws<SlicedModel.SliceNotFoundException>(() => store3.GetState());

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