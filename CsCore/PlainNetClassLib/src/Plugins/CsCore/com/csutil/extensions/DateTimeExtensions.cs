using System;
using System.Globalization;

namespace com.csutil {

    public static class DateTimeExtensions {

        public static bool IsBetween(this DateTime self, DateTime lowerBound, DateTime upperBound) {
            return self.Ticks >= lowerBound.Ticks && self.Ticks <= upperBound.Ticks;
        }

        /// <summary> sortable, short, hard to read wrong, can be used in pathes. read also https://stackoverflow.com/a/15952652/165106 </summary>
        public static string ToReadableString(this DateTime self) {
            return self.ToUniversalTime().ToString("yyyy-MM-dd_HH.mm");
        }

        /// <summary> sortable, short, hard to read wrong, can be used in pathes. read also https://stackoverflow.com/a/15952652/165106 </summary>
        public static string ToLocalUiString(this DateTime self) {
            return self.ToLocalTime().ToString("MMMM dd, yyyy " + DateTimeFormatInfo.CurrentInfo.ShortTimePattern);
        }
        
        public static string ToLocalUiString(this DateTimeOffset self) {
            return self.UtcDateTime.ToLocalUiString();
        }
        
        /// <summary> sortable, short, hard to read wrong, can be used in pathes. read also https://stackoverflow.com/a/15952652/165106 </summary>
        public static string ToLocalUiStringV2(this DateTime self) {
            return self.ToLocalTime().ToString("MMMM dd, yyyy " + DateTimeFormatInfo.CurrentInfo.LongTimePattern);
        }
        
        public static string ToLocalUiStringV2(this DateTimeOffset self) {
            return self.UtcDateTime.ToLocalUiStringV2();
        }

        /// <summary> e.g. 2019-10-27T13:35:47Z (see https://en.wikipedia.org/wiki/ISO_8601 for details) </summary>
        public static string ToReadableString_ISO8601(this DateTime dateTime) {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        public static string ToReadableStringExact(this DateTime self) {
            return self.ToString("yyyy-MM-dd_HH.mm.ss.fff");
        }
        
        public static long ToUnixTimestampUtc(this DateTime self) {
            var zero = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64(Math.Truncate((self.ToUniversalTime().Subtract(zero)).TotalMilliseconds));
        }

        public static bool IsBefore(this DateTime self, DateTime other) {
            return self.ToUnixTimestampUtc() < other.ToUnixTimestampUtc();
        }

        public static bool IsAfter(this DateTime self, DateTime other) {
            return self.ToUnixTimestampUtc() > other.ToUnixTimestampUtc();
        }

        public static bool IsUtc(this DateTime self) { return self.Kind == DateTimeKind.Utc; }

        public static DateTime GetLatestLaunchDate(this EnvironmentV2.ISystemInfo self) {
            return DateTimeV2.NewDateTimeFromUnixTimestamp(self.latestLaunchDate);
        }

        public static DateTime? GetFirstLaunchDate(this EnvironmentV2.ISystemInfo self) {
            if (self.firstLaunchDate == null) { return null; }
            return DateTimeV2.NewDateTimeFromUnixTimestamp(self.firstLaunchDate.Value);
        }

        public static DateTime? GetLastUpdateDate(this EnvironmentV2.ISystemInfo self) {
            if (self.lastUpdateDate == null) { return null; }
            return DateTimeV2.NewDateTimeFromUnixTimestamp(self.lastUpdateDate.Value);
        }

        public static double TotalMillisecondsAbs(this TimeSpan self) {
            return Math.Abs(self.TotalMilliseconds);
        }

    }

}