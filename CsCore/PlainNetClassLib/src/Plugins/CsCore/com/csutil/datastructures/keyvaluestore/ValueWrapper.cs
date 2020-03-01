using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public class ValueWrapper {

        public object value;
        public long lastModified = DateTime.Now.ToUnixTimestampUtc();

        public T GetValueAs<T>() {
            if ((typeof(T).IsEnum)) { return (T)Enum.Parse(typeof(T), value.ToString()); }
            return (T)value;
        }
    }

}
