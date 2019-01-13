using System.Collections.Generic;

namespace com.csutil.json {

    public interface HandleAdditionalJsonFields {
        Dictionary<string, object> GetMissingFields();
        void SetMissingFields(Dictionary<string, object> missingFields);
    }
}