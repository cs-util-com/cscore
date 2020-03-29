using System;
using System.Collections.Generic;

namespace com.csutil {

    /// <summary>
    /// This class will use the received external UTC times from any performed web requests to correct any local incorrect time settings. 
    /// DateTimeV2.UtcNow is a direct replacement for DateTime.UtcNow and will return once the first web request was performed a corrected 
    /// UTC timestamp that ignores if the local system time is set to an incorrect value. This ensures that timestamps recorded on clients 
    /// are always correct even if the user is using a manually set system time.
    /// </summary>
    public class DateTimeV2 : IDisposable {

        public const string SERVER_UTC_DATE = "SERVER_UTC_DATE";

        public static DateTime UtcNow { get { return IoC.inject.GetOrAddSingleton<DateTimeV2>(null).GetUtcNow(); } }

        public static DateTime Now { get { return IoC.inject.GetOrAddSingleton<DateTimeV2>(null).GetNow(); } }

        public ISet<string> uriBlacklist = new HashSet<string>();
        public TimeSpan? diffOfLocalToServer { get; private set; }

        public DateTimeV2() { EventBus.instance.Subscribe(this, SERVER_UTC_DATE, (Uri uri, DateTime utcDate) => onUtcUpdate(uri, utcDate)); }
        public void Dispose() { EventBus.instance.UnsubscribeAll(this); }

        private void onUtcUpdate(Uri uri, DateTime serverUtcDate) {
            try {
                if (!uriBlacklist.Contains(uri.Host)) { diffOfLocalToServer = serverUtcDate - DateTime.UtcNow; }
            } catch (Exception e) { Log.e("Error when processing server utc date from " + uri, e); }
        }

        public DateTime GetUtcNow() {
            if (diffOfLocalToServer != null) { return DateTime.UtcNow + diffOfLocalToServer.Value; }
            return DateTime.UtcNow;
        }

        public DateTime GetNow() {
            if (diffOfLocalToServer != null) { return DateTime.Now + diffOfLocalToServer.Value; }
            return DateTime.Now;
        }

    }

}