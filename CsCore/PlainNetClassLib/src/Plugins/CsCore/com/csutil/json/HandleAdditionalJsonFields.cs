using System.Collections.Generic;

namespace com.csutil.json {

    public interface HandleAdditionalJsonFields {

        Dictionary<string, object> AdditionalJsonFields { get; set; }

    }
}