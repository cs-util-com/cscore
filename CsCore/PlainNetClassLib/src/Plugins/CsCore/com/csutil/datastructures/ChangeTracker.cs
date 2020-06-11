namespace com.csutil.datastructures {

    public class ChangeTracker<T> {

        public T value { get; private set; }

        public ChangeTracker(T startValue) { value = startValue; }

        public bool SetNewValue(T t) {
            if (Equals(t, value)) { return false; }
            value = t; return true;
        }

    }

}