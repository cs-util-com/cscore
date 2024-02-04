using System;
using System.Collections.Generic;

namespace com.csutil {
    
    public class UniqueStack<T> : IEnumerable<T> {
        
        private HashSet<T> hashSet;
        private Stack<T> stack;

        public UniqueStack() {
            hashSet = new HashSet<T>();
            stack = new Stack<T>();
        }

        public UniqueStack(IEnumerable<T> source) : this() {
            foreach (var item in source) {
                Push(item);
            }
        }

        public int Count => hashSet.Count;

        public void Clear() {
            hashSet.Clear();
            stack.Clear();
        }

        public bool Contains(T item) {
            return hashSet.Contains(item);
        }

        public void Push(T item) {
            if (hashSet.Add(item)) {
                stack.Push(item);
            } else {
                throw new InvalidOperationException("The item is already in the stack");
            }
        }

        public T Pop() {
            T item = stack.Pop();
            hashSet.Remove(item);
            return item;
        }

        public T Peek() {
            return stack.Peek();
        }

        public IEnumerator<T> GetEnumerator() {
            return stack.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return stack.GetEnumerator();
        }
        
    }
    
}