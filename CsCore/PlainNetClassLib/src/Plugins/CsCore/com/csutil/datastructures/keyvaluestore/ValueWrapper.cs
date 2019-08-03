using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public class ValueWrapper {

        public object value;
        public long lastModified = new DateTime().ToUnixTimestampUtc();

    }

}
