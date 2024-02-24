using System;
using System.Collections.Generic;
using com.csutil.keyvaluestore;

namespace com.csutil.logging.analytics {

    public interface ILocalAnalytics : IKeyValueStoreTypeAdapter<AppFlowEvent>, IDisposableV2 {
        IReadOnlyDictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>> categoryStores { get; }
        KeyValueStoreTypeAdapter<AppFlowEvent> GetStoreForCategory(string catMethod);
    }

}