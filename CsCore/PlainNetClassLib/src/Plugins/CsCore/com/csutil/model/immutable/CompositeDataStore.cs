namespace com.csutil.model.immutable {

    public class CompositeDataStore<M1, M2> : DataStore<M1>, IDataStore<M2> {

        private readonly IDataStore<M2> model2Store;

        public CompositeDataStore(StateReducer<M1> model1Reducer, Middleware<M1>[] model1Middlewares, M1 initialModel1State,
            StateReducer<M2> model2Reducer, M2 initialModel2State, params Middleware<M2>[] model2Middlewares)
            : this(model1Reducer, model1Middlewares, initialModel1State, new DataStore<M2>(model2Reducer, initialModel2State, model2Middlewares)) {
        }

        public CompositeDataStore(StateReducer<M1> model1Reducer, Middleware<M1>[] model1Middlewares, M1 initialModel1State,
            IDataStore<M2> model2Store) : base(model1Reducer, initialModel1State, model1Middlewares) {
            this.model2Store = model2Store;
        }

        public new M2 GetState() { return model2Store.GetState(); }
        
        public IDataStore<M2> GetInnerStore() { return model2Store; }

        public override object Dispatch(object action) {
            var o2 = model2Store.Dispatch(action);
            // Calling base.Dispatch will internally also trigger the shared onStateChanged event:
            var o1 = base.Dispatch(action);
            if (ReferenceEquals(o2, action)) { return o1; }
            return o2;
        }

        public override void Destroy() {
            if (model2Store is DataStore<M2> d) { d.Destroy(); }
            base.Destroy();
        }

    }

}