using System.Collections.Generic;

namespace com.csutil {

    public class UniqueQueue<T> : IEnumerable<T> {

        private HashSet<T> hashSet;
        private Queue<T> queue;
        private static object mutateLock = new object();

        public UniqueQueue() {
            hashSet = new HashSet<T>();
            queue = new Queue<T>();
        }

        public UniqueQueue(IEnumerable<T> source) : this() {
            foreach (var item in source) {
                Enqueue(item);
            }
        }

        public int Count => hashSet.Count;


        public void Clear() {
            lock (mutateLock) {
                hashSet.Clear();
                queue.Clear();
            }
        }


        public bool Contains(T item) {
            return hashSet.Contains(item);
        }


        public void Enqueue(T item) {
            lock (mutateLock) {
                if (hashSet.Add(item)) {
                    queue.Enqueue(item);
                } else {
                    throw new System.InvalidOperationException("The item is already in the queue");
                }
            }
        }

        public T Dequeue() {
            lock (mutateLock) {
                T item = queue.Dequeue();
                hashSet.Remove(item);
                return item;
            }
        }


        public T Peek() {
            return queue.Peek();
        }


        public IEnumerator<T> GetEnumerator() {
            return queue.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return queue.GetEnumerator();
        }

    }

}