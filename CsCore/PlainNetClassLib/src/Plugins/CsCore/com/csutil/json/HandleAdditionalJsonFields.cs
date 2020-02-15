using System;
using System.Collections.Generic;

namespace com.csutil.json {

    // TODO mark obsolete if JsonExtensionData annotation works correct in WebGL:
    // [Obsolete("Use JsonExtensionData annotation instead, example at https://www.newtonsoft.com/json/help/html/DeserializeExtensionData.htm")]
    public interface HandleAdditionalJsonFields {

        Dictionary<string, object> GetAdditionalJsonFields();
        void SetAdditionalJsonFields(Dictionary<string, object> additionalJsonFields);

    }
}