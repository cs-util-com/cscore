using System;
using System.Collections.Generic;

namespace com.csutil {

    /// <summary>
    /// This class will use the received external UTC times from any performed web requests to correct any local incorrect time settings. 
    /// DateTimeV2.UtcNow is a direct replacement for DateTime.UtcNow and will return once the first web request was performed a corrected 
    /// UTC timestamp that ignores if the local system time is set to an incorrect value. This ensures that timestamps recorded on clients 
    /// are always correct even if the user is using a manually set system time.
    /// </summary>
    public class DateTimeV2 : IDisposableV2 {

        public static DateTime NewDateTimeFromUnixTimestamp(long unixTimeInMs, bool autoCorrectIfPassedInSeconds = true) {
            AssertV2.IsTrue(unixTimeInMs > 0, "NewDateTimeFromUnixTimestamp: unixTimeInMs was " + unixTimeInMs);
            DateTime result = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeInMs).UtcDateTime;
            if (result.Year == 1970) {
                if (autoCorrectIfPassedInSeconds) {
                    return DateTimeOffset.FromUnixTimeSeconds(unixTimeInMs).UtcDateTime;
                } else {
                    Log.e("The passed unixTimeInMs was likely passed in seconds instead of milliseconds,"
                        + " it was too small by a factor of *1000 and will result in " + result.ToReadableString());
                }
            }
            return result;
        }

        /// <summary> Parses a date string to a DateTime object </summary>
        /// <param name="utcTime"> e.g. "22.03.2011 01:26:00" </param>
        public static DateTime ParseUtc(string utcTime) {
            if (long.TryParse(utcTime, out long unixTimeInMs)) { return NewDateTimeFromUnixTimestamp(unixTimeInMs); }
            if (utcTime.Contains("GMT")) {
                utcTime = utcTime.Substring(0, "GMT", true);
            } else if (utcTime.Contains("UTC")) {
                // RFC1123Pattern expects GMT and crashes on UTC
                utcTime = utcTime.Substring(0, "UTC", false) + "GMT";
            }
            var time = DateTime.Parse(utcTime);
            if (time.Kind == DateTimeKind.Unspecified) { return DateTime.SpecifyKind(time, DateTimeKind.Utc); }
            return time.ToUniversalTime();
        }

        public static DateTime ParseLocalTime(string localTimeString) {
            return DateTime.Parse(localTimeString);
        }

        public const string SERVER_UTC_DATE = "SERVER_UTC_DATE";

        public static DateTime UtcNow { get { return IoC.inject.GetOrAddSingleton<DateTimeV2>(null).GetUtcNow(); } }

        public static DateTime Now { get { return IoC.inject.GetOrAddSingleton<DateTimeV2>(null).GetNow(); } }

        public ISet<string> uriBlacklist = new HashSet<string>();

        /// <summary> Contains the time difference between the UTC time reported by the backend and the local clock </summary>
        public TimeSpan? diffOfLocalToServer { get; private set; }

        /// <summary> This flag is set to false every time a new REST request reported a current UTC time from a backend. Can be switched back to true to repeat the remote time update </summary>
        public bool RequestUpdateOfDiffOfLocalToServer = true;

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;
        
        /// <summary> Can be overwritten, by default any remote time that is max 5sec different to local time is accepted </summary>
        public Func<TimeSpan, bool> IsAcceptableDistanceToLocalTime = (diff) => diff.TotalMillisecondsAbs() < 5000;

        public DateTimeV2() {
            EventBus.instance.Subscribe(this, SERVER_UTC_DATE, (Uri uri, DateTime utcDate) => onUtcUpdate(uri, utcDate));
        }

        public void Dispose() {
            IsDisposed = DisposeState.DisposingStarted;
            EventBus.instance.UnsubscribeAll(this);
            if (IoC.inject.Get<DateTimeV2>(this, false) == this) { IoC.inject.RemoveAllInjectorsFor<DateTimeV2>(); }
            IsDisposed = DisposeState.Disposed;
        }

        private void onUtcUpdate(Uri uri, DateTime serverUtcDate) {
            try {
                if (!uriBlacklist.Contains(uri.Host) && RequestUpdateOfDiffOfLocalToServer) {
                    diffOfLocalToServer = serverUtcDate - DateTime.UtcNow;
                    // Log.d($"Switching from local clock ({DateTime.UtcNow.ToReadableStringExact()}) to server clock ({serverUtcDate.ToReadableStringExact()}), diffOfLocalToServer={diffOfLocalToServer}");
                    if (IsAcceptableDistanceToLocalTime(diffOfLocalToServer.Value)) { diffOfLocalToServer = null; }
                    RequestUpdateOfDiffOfLocalToServer = false;
                }
            } catch (Exception e) { Log.w("Error when processing server utc date from " + uri, e); }
        }

        public virtual DateTime GetUtcNow() {
            if (diffOfLocalToServer != null) { return DateTime.UtcNow + diffOfLocalToServer.Value; }
            return DateTime.UtcNow;
        }

        public virtual DateTime GetNow() {
            if (diffOfLocalToServer != null) { return DateTime.Now + diffOfLocalToServer.Value; }
            return DateTime.Now;
        }

    }

}