using System.Collections.Generic;

namespace com.csutil.model.immutable {

    public class RedoAction<T> { }
    public class UndoAction<T> { }
    
    /// <summary> An action that should be ignored/not recorded by the <see cref="UndoRedoReducer{T}"/> should be marked with 
    /// this interface. Examples where this is useful are processing or loading states that should not be undoable, eg a 
    /// download process where every single percentage change should not have its own undo step </summary>
    public interface SkipInUndoRedoLogic { }

    /// <summary> This is a higher order reducer that can wrap any inner reducer to enable undo redo functionallity </summary>
    public class UndoRedoReducer<T> {

        private Stack<T> past = new Stack<T>();
        private Stack<T> future = new Stack<T>();

        public StateReducer<T> Wrap(StateReducer<T> wrappedReducer) {
            return (T present, object action) => {
                if (action is UndoAction<T>) { return RestoreFrom(past, present, future); }
                if (action is RedoAction<T>) { return RestoreFrom(future, present, past); }
                var newPresent = wrappedReducer(present, action);
                if (!(action is SkipInUndoRedoLogic)) {
                    past.Push(present);
                    future.Clear();
                }
                return newPresent;
            };
        }

        private static T RestoreFrom(Stack<T> source, T present, Stack<T> target) {
            var newPresent = source.Pop();
            target.Push(present);
            return newPresent;
        }

    }

}