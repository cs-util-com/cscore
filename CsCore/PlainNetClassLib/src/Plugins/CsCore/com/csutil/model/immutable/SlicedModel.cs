using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;

namespace com.csutil.model.immutable {

    public static class SlicedStoreExtensions {

        public static SubState<T, T> GetState<T>(this IDataStore<SlicedModel> store) {
            return store.GetStore<T>().GetSubState(x => x);
        }

        public static SubState<T, S> GetSubState<T, S>(this IDataStore<SlicedModel> store, Func<T, S> getSubState) {
            return store.GetStore<T>().GetSubState(getSubState);
        }

        public static IDataStore<T> GetStore<T>(this IDataStore<SlicedModel> store) {
            return new SlicedStore<T>(store);
        }

        private class SlicedStore<T> : IDataStore<T> {

            private readonly IDataStore<SlicedModel> Store;
            public SlicedStore(IDataStore<SlicedModel> store) {
                this.Store = store;
            }
            public Action onStateChanged { get => Store.onStateChanged; set => Store.onStateChanged = value; }

            public T GetState() {
                return Store.GetState().GetSlice<T>();
            }

            public object Dispatch(object action) {
                return Store.Dispatch(action);
            }

        }

    }

    public class SlicedModel {

        public class Slice {

            public static Slice New<T>(T initialState, StateReducer<T> reducer) {
                return new Slice(initialState, (object oldState, object action) => reducer((T)oldState, action));
            }

            public readonly object Model;
            private readonly StateReducer<object> MyReducer;

            private Slice(object model, StateReducer<object> reducer) {
                Model = model;
                MyReducer = reducer;
            }

            public static Slice Reduce(Slice slice, object action) {
                var newModel = slice.MyReducer(slice.Model, action);
                if (StateCompare.WasModified(slice.Model, newModel)) {
                    return New(newModel, slice.MyReducer);
                }
                return slice;
            }
        }

        public readonly ImmutableList<Slice> Slices;
        private readonly ImmutableDictionary<Type, Slice> _getSliceLookUpTable;

        public SlicedModel(IEnumerable<Slice> slices) : this(slices.ToImmutableList()) { }

        [JsonConstructor]
        private SlicedModel(ImmutableList<Slice> slices) {
            Slices = slices;
            _getSliceLookUpTable = slices.ToImmutableDictionary(x => x.Model.GetType(), x => x);
        }

        public T GetSlice<T>() {
            return (T)_getSliceLookUpTable[typeof(T)].Model;
        }

        public static SlicedModel Reducer(SlicedModel previousstate, object action) {
            var changed = false;
            var newSlices = previousstate.Slices.MutateEntries(action, Slice.Reduce, ref changed);
            return changed ? new SlicedModel(newSlices) : previousstate;
        }

    }

}