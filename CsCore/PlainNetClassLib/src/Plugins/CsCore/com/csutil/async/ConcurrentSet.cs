using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace com.csutil {

    public class ConcurrentSet<T> : ISet<T> {

        public int Count => storage.Count;

        private ConcurrentDictionary<T, byte> storage;

        public ConcurrentSet() {
            storage = new ConcurrentDictionary<T, byte>();
        }

        public ConcurrentSet(IEnumerable<T> collection) {
            storage = new ConcurrentDictionary<T, byte>(collection.Select(_ => new KeyValuePair<T, byte>(_, 0)));
        }

        public ConcurrentSet(IEqualityComparer<T> comparer) {
            storage = new ConcurrentDictionary<T, byte>(comparer);
        }

        public ConcurrentSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) {
            storage = new ConcurrentDictionary<T, byte>(collection.Select(_ => new KeyValuePair<T, byte>(_, 0)), comparer);
        }

        public ConcurrentSet(int concurrencyLevel, int capacity) {
            storage = new ConcurrentDictionary<T, byte>(concurrencyLevel, capacity);
        }

        public ConcurrentSet(int concurrencyLevel, IEnumerable<T> collection, IEqualityComparer<T> comparer) {
            storage = new ConcurrentDictionary<T, byte>(concurrencyLevel, collection.Select(_ => new KeyValuePair<T, byte>(_, 0)), comparer);
        }

        public ConcurrentSet(int concurrencyLevel, int capacity, IEqualityComparer<T> comparer) {
            storage = new ConcurrentDictionary<T, byte>(concurrencyLevel, capacity, comparer);
        }

        public bool Contains(T item) { return storage.ContainsKey(item); }

        public bool TryRemove(T item) {
            byte dontCare;
            return storage.TryRemove(item, out dontCare);
        }

        void ICollection<T>.Add(T item) {
            ((ICollection<KeyValuePair<T, byte>>)storage).Add(new KeyValuePair<T, byte>(item, 0));
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
            foreach (KeyValuePair<T, byte> pair in storage) {
                array[arrayIndex++] = pair.Key;
            }
        }

        bool ICollection<T>.IsReadOnly => false;

        bool ICollection<T>.Remove(T item) { return TryRemove(item); }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return storage.Keys.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return storage.Keys.GetEnumerator(); }

        public bool Add(T item) { return storage.TryAdd(item, 0); }

        public void Clear() { storage.Clear(); }

        public bool SetEquals(IEnumerable<T> other) {
            if (other is null) { return false; }
            if (ReferenceEquals(other, this)) { return true; }
            HashSet<T> collected = new HashSet<T>();
            foreach (var k in other) {
                if (!storage.ContainsKey(k)) { return false; }
                if (!collected.Add(k)) { return false; } // Check for duplicates
            }
            return collected.Count == storage.Count;
        }

        public override bool Equals(object obj) {
            if (obj is IEnumerable<T> other) { return SetEquals(other); }
            return base.Equals(obj);
        }
        
        public void ExceptWith(IEnumerable<T> other) { throw new System.NotImplementedException(); }
        public void IntersectWith(IEnumerable<T> other) { throw new System.NotImplementedException(); }
        public bool IsProperSubsetOf(IEnumerable<T> other) { throw new System.NotImplementedException(); }
        public bool IsProperSupersetOf(IEnumerable<T> other) { throw new System.NotImplementedException(); }
        public bool IsSubsetOf(IEnumerable<T> other) { throw new System.NotImplementedException(); }
        public bool IsSupersetOf(IEnumerable<T> other) { throw new System.NotImplementedException(); }
        public bool Overlaps(IEnumerable<T> other) { throw new System.NotImplementedException(); }
        public void SymmetricExceptWith(IEnumerable<T> other) { throw new System.NotImplementedException(); }
        public void UnionWith(IEnumerable<T> other) { throw new System.NotImplementedException(); }

    }

}