using System;
using System.Collections.Generic;
using com.csutil.keyvaluestore;

namespace com.csutil.logging.analytics {

    public interface ILocalAnalytics : IKeyValueStoreTypeAdapter<AppFlowEvent>, IDisposable {
        IReadOnlyDictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>> categoryStores { get; }
        KeyValueStoreTypeAdapter<AppFlowEvent> GetStoreForCategory(string catMethod);
    }

}