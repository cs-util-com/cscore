using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace com.csutil {

    public class FixedSizedQueue<T> : IEnumerable<T> {
        // See https://stackoverflow.com/a/5852926/165106 why Queue is not extended here

        ConcurrentQueue<T> q = new ConcurrentQueue<T>();

        private object lockObject = new object();

        public int fixedSize { get; }

        public int Count => q.Count;

        public FixedSizedQueue(int size) { fixedSize = size; }

        public void Enqueue(T obj) {
            q.Enqueue(obj);
            lock (lockObject) {
                T _;
                while (q.Count > fixedSize && q.TryDequeue(out _)) { }
            }
        }

        public T Dequeue() {
            T obj;
            q.TryDequeue(out obj);
            return obj;
        }

        public IEnumerator<T> GetEnumerator() {
            return q.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return q.GetEnumerator();
        }

    }

}