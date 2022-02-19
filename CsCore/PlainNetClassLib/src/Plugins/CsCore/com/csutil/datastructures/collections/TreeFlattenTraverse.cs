using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil {

    /// <summary> Walks through an arbitrary tree datastructure and flattens it (collects all
    /// children on the way in an IEnumerable). Both breadth and depth first traversal possible </summary>
    public static class TreeFlattenTraverse {

        public static IEnumerable<T> BreadthFirst<T>(T root, Func<T, IEnumerable<T>> childrenSelector) {
            var queue = new Queue<T>();
            queue.Enqueue(root);
            return BreadthFirst(queue, childrenSelector).Cached();
        }

        public static IEnumerable<T> BreadthFirst<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector) {
            return BreadthFirst(new Queue<T>(source), childrenSelector).Cached();
        }

        private static IEnumerable<T> BreadthFirst<T>(Queue<T> queue, Func<T, IEnumerable<T>> childrenSelector) {
            // From https://stackoverflow.com/a/63967111/165106
            while (queue.Count > 0) {
                var current = queue.Dequeue();
                yield return current;
                var children = childrenSelector(current);
                if (children == null) continue;
                foreach (var child in children) { queue.Enqueue(child); }
            }
        }

        public static IEnumerable<T> DepthFirst<T>(T root, Func<T, IEnumerable<T>> childrenSelector) {
            return DepthFirst(new T[1] { root }, childrenSelector).Cached();
        }

        public static IEnumerable<T> DepthFirst<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector) {
            // From https://stackoverflow.com/a/31881243/165106
            var e = source.GetEnumerator();
            var stack = new Stack<IEnumerator<T>>();
            try {
                while (true) {
                    while (e.MoveNext()) {
                        var item = e.Current;
                        yield return item;
                        var elements = childrenSelector(item);
                        if (elements == null) { continue; }
                        stack.Push(e);
                        e = elements.GetEnumerator();
                    }
                    if (stack.Count == 0) { break; }
                    e.Dispose();
                    e = stack.Pop();
                }
            }
            finally {
                e.Dispose();
                while (stack.Count != 0) { stack.Pop().Dispose(); }
            }
        }

    }

}