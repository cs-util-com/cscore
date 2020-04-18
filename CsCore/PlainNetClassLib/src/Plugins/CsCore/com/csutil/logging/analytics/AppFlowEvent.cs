namespace com.csutil.logging.analytics {

    public class AppFlowEvent {
        public long time { get; set; } = DateTimeV2.UtcNow.ToUnixTimestampUtc();
        public string cat { get; set; }
        public string action { get; set; }
        public object[] args { get; set; }
    }

}