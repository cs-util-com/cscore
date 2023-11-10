using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;

namespace com.csutil.model.immutable {

    public static class SlicedStoreExtensions {

        public static void AddSlice<T>(this IDataStore<SlicedModel> store, T initialState, StateReducer<T> reducer) {
            SlicedModel.Slice.New(initialState, reducer).AddToStore(store);
        }

        public static void RemoveSlice(this IDataStore<SlicedModel> store, SlicedModel.Slice sliceToRemove) {
            sliceToRemove.RemoveFromStore(store);
        }

        public static SubState<T, T> GetState<T>(this IDataStore<SlicedModel> store) {
            return store.GetStore<T>().GetSubState(x => x);
        }

        public static SubState<T, S> GetSubState<T, S>(this IDataStore<SlicedModel> store, Func<T, S> getSubState) {
            return store.GetStore<T>().GetSubState(getSubState);
        }

        /// <summary> Returns a fully functional store to interact with the specified slice </summary>
        /// <typeparam name="T"> The specified slice </typeparam>
        public static IDataStore<T> GetStore<T>(this IDataStore<SlicedModel> store) {
            return new SlicedStore<T>(store);
        }

        private class SlicedStore<T> : IDataStore<T> {

            private readonly IDataStore<SlicedModel> _slicedStore;
            public SlicedStore(IDataStore<SlicedModel> slicedStore) { _slicedStore = slicedStore; }

            public Action onStateChanged { get => _slicedStore.onStateChanged; set => _slicedStore.onStateChanged = WrapWithRemoveHandler(value); }
            private Action WrapWithRemoveHandler(Action listener) {
                return () => {
                    try {
                        listener();
                    } catch (SlicedModel.SliceNotFoundException e) {
                        // If the slice was removed from the store then remove the listener:
                        if (e.SliceType != typeof(T)) {
                            throw;
                        }
                    }
                };
            }

            public T GetState() { return _slicedStore.GetState().GetSlice<T>(); }

            public object Dispatch(object action) { return _slicedStore.Dispatch(action); }

        }

    }

    public class SlicedModel {

        private class ActionAddStoreSlice {
            public readonly Slice Slice;
            [JsonConstructor]
            public ActionAddStoreSlice(Slice slice) { Slice = slice; }
        }

        private class ActionRemoveStoreSlice {
            public readonly Slice Slice;
            [JsonConstructor]
            public ActionRemoveStoreSlice(Slice slice) { Slice = slice; }
        }

        public class Slice {

            public void AddToStore(IDataStore<SlicedModel> store) {
                store.Dispatch(new ActionAddStoreSlice(this));
            }

            public void RemoveFromStore(IDataStore<SlicedModel> store) {
                store.Dispatch(new ActionRemoveStoreSlice(this));
            }

            public static Slice New<T>(T initialState, StateReducer<T> reducer) {
                return new Slice(initialState, (object oldState, object action) => reducer((T)oldState, action));
            }

            public readonly object Model;
            private readonly StateReducer<object> MyReducer;

            [JsonConstructor]
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
            if (_getSliceLookUpTable.TryGetValue(typeof(T), out Slice slice)) {
                return (T)slice.Model;
            } else {
                throw new SliceNotFoundException(typeof(T));
            }
        }

        public bool TryGetSlice<T>(out T slice) {
            if (_getSliceLookUpTable.TryGetValue(typeof(T), out Slice s)) {
                slice = (T)s.Model;
                return true;
            } else {
                slice = default(T);
                return false;
            }
        }

        public static SlicedModel Reducer(SlicedModel previousstate, object action) {
            var changed = false;
            if (action is ActionAddStoreSlice a) {
                if (previousstate.Slices.Contains(a.Slice)) {
                    throw new ArgumentException("Slice already added to store");
                }
                return new SlicedModel(previousstate.Slices.Add(a.Slice));
            }
            if (action is ActionRemoveStoreSlice r) {
                if (!previousstate.Slices.Contains(r.Slice)) {
                    throw new ArgumentException("Cant remove slice that is not part of the store");
                }
                return new SlicedModel(previousstate.Slices.Remove(r.Slice));
            }
            var newSlices = previousstate.Slices.MutateEntries(action, Slice.Reduce, ref changed);
            return changed ? new SlicedModel(newSlices) : previousstate;
        }

        public class SliceNotFoundException : Exception {

            public readonly Type SliceType;

            public SliceNotFoundException(Type sliceType) : base("Slice not found: " + sliceType) {
                SliceType = sliceType;
            }

        }

    }

}