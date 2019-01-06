using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil.datastructures {

    public class ChangeTracker<T> {
        public T value { get; private set; }
        public ChangeTracker(T startValue) { value = startValue; }
        public bool setNewValue(T t) { if (Equals(t, value)) { return false; } value = t; return true; }
    }

}
