using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    public class DataStoreExample7_StoreSlicing {

        public DataStoreExample7_StoreSlicing(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            var slices = new SlicedModel(new[] {
                SlicedModel.Slice.New(new Model1(), Model1Reducer),
                SlicedModel.Slice.New(new Model2(), Model2Reducer),
                SlicedModel.Slice.New(new Model3(), Model3Reducer)
            });
            DataStore<SlicedModel> store = new DataStore<SlicedModel>(SlicedModel.Reducer, slices, Middlewares.NewLoggingMiddleware<SlicedModel>());
            IDataStore<Model1> store1 = store.GetStore<Model1>();
            IDataStore<Model2> store2 = store.GetStore<Model2>();
            IDataStore<Model3> store3 = store.GetStore<Model3>();
            Assert.Null(store1.GetState().a);
            Assert.Null(store2.GetState().b);
            Assert.Null(store3.GetState().c);
            store1.Dispatch(new ActionChangeA() { newA = "a" });
            Assert.Equal("a", store1.GetState().a);
            Assert.Null(store2.GetState().b);
            Assert.Null(store3.GetState().c);

            var slice1 = store.GetState().GetSlice<Model1>();
            var slice2 = store.GetState().GetSlice<Model2>();
            Assert.Equal("a", slice1.a);
            store2.Dispatch(new ActionChangeB() { newB = "b" });
            Assert.Same(slice1, store.GetState().GetSlice<Model1>());
            Assert.NotSame(slice2, store.GetState().GetSlice<Model2>());
            Assert.Equal("a", store1.GetState().a);
            Assert.Equal("b", store2.GetState().b);
            Assert.Null(store3.GetState().c);

            store3.Dispatch(new ActionChangeC() { newC = "c" });
            Assert.Equal("a", store1.GetState().a);
            Assert.Equal("b", store2.GetState().b);
            Assert.Equal("c", store3.GetState().c);

            // GetState can return the slice of the store:
            var model2 = store.GetState<Model2>();
            Assert.Equal("b", model2.GetState().b);
            // GetSubState can automatically access the correct slice of the store:
            var model3 = store.GetSubState((Model3 x) => x.c);
            Assert.Equal("c", model3.GetState());

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