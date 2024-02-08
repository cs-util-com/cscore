using System;
using Newtonsoft.Json;

namespace com.csutil.logging.analytics {

    public class AppFlowEvent {
        public long time { get; set; } = DateTimeV2.UtcNow.ToUnixTimestampUtc();
        public string cat { get; set; }
        public string action { get; set; }
        
        [JsonIgnore]
        public object[] args { get; set; }

        public DateTime GetDateTimeUtc() { return DateTimeV2.NewDateTimeFromUnixTimestamp(time); }

    }

}