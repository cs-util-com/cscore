using System;
using System.Collections.Generic;

namespace com.csutil.model.immutable {

    public class RedoAction { }
    public class UndoAction { }

    /// <summary> This is a higher order reducer that can wrap any inner reducer to enable undo redo functionallity </summary>
    public class UndoRedoReducer<T> {
        private Stack<T> past = new Stack<T>();
        private Stack<T> future = new Stack<T>();

        public StateReducer<T> wrap(StateReducer<T> wrappedReducer) {
            return (T present, object action) => {
                if (action is UndoAction) { return Undo(present); }
                if (action is RedoAction) { return Redo(present); }
                var newPresent = wrappedReducer(present, action);
                past.Push(present);
                future.Clear();
                return newPresent;
            };
        }

        private T Redo(T present) {
            var newPresent = future.Pop();
            past.Push(present);
            return newPresent;
        }

        private T Undo(T present) {
            var newPresent = past.Pop();
            future.Push(present);
            return newPresent;
        }
    }

}