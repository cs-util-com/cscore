using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;

namespace com.csutil.model.immutable {

    public static class SlicedStoreExtensions {

        public static SubState<SlicedModel, T> GetSlice<T>(this IDataStore<SlicedModel> store) {
            return store.GetSubState(x => x.GetSlice<T>());
        }

    }

    public class SlicedModel {

        public class Slice {

            public static Slice New<T>(T model, StateReducer<T> reducer) {
                return new Slice(model, (object oldState, object action) => reducer((T)oldState, action));
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
        private readonly ImmutableDictionary<Type, Slice> _slices;

        public SlicedModel(IEnumerable<Slice> slices) : this(slices.ToImmutableDictionary(x => x.Model.GetType(), x => x)) { }

        private SlicedModel(ImmutableDictionary<Type, Slice> slices) : this(slices.Values.ToImmutableList(), slices) { }

        [JsonConstructor]
        private SlicedModel(ImmutableList<Slice> slicesList, ImmutableDictionary<Type, Slice> slicesDict) {
            Slices = slicesList;
            _slices = slicesDict;
        }

        public T GetSlice<T>() {
            Slice slice = _slices[typeof(T)];
            return (T)slice.Model;
        }

        public static SlicedModel Reducer(SlicedModel previousstate, object action) {
            bool changed = false;
            var newSlices = previousstate._slices.MutateEntries(action, Slice.Reduce, ref changed);
            if (changed) {
                return new SlicedModel(newSlices);
            }
            return previousstate;
        }

    }

}