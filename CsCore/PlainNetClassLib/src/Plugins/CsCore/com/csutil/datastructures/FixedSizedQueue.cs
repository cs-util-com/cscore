using System.Collections.Generic;

namespace com.csutil {

    public class FixedSizedQueue<T> : Queue<T> {

        public int Size { get; private set; }

        public FixedSizedQueue(int size) { Size = size; }

        public new FixedSizedQueue<T> Enqueue(T obj) {
            base.Enqueue(obj);
            while (base.Count > Size) { base.Dequeue(); }
            return this;
        }
    }

}