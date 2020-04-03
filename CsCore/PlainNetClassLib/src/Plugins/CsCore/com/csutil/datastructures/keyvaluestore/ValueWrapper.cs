using System;

namespace com.csutil.keyvaluestore {

    public class ValueWrapper {

        public object value;
        public long lastModified = DateTimeV2.UtcNow.ToUnixTimestampUtc();

        public T GetValueAs<T>() {
            if ((typeof(T).IsEnum)) { return (T)Enum.Parse(typeof(T), value.ToString()); }
            return (T)value;
        }

    }

}
