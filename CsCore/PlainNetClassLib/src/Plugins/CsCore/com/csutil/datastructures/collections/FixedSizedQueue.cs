using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace com.csutil {

    public class FixedSizedQueue<T> {
        // See https://stackoverflow.com/a/5852926/165106 why Queue is not extended here

        ConcurrentQueue<T> q = new ConcurrentQueue<T>();

        private object lockObject = new object();

        public int fixedSize { get; private set; }

        public int Count { get { return q.Count; } }

        public FixedSizedQueue(int size) { this.fixedSize = size; }

        public void Enqueue(T obj) {
            q.Enqueue(obj);
            lock (lockObject) {
                T _; while (q.Count > fixedSize && q.TryDequeue(out _)) ;
            }
        }

        public T Dequeue() { T obj; q.TryDequeue(out obj); return obj; }

    }


}