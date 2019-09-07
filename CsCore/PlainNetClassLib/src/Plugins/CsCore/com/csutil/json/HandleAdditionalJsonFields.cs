using System.Collections.Generic;

namespace com.csutil.json {

    public interface HandleAdditionalJsonFields {

        Dictionary<string, object> GetAdditionalJsonFields();
        void SetAdditionalJsonFields(Dictionary<string, object> additionalJsonFields);

    }
}