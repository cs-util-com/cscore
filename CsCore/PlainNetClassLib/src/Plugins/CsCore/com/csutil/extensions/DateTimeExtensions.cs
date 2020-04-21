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

        /// <summary> e.g. 2019-10-27T13:35:47Z (see https://en.wikipedia.org/wiki/ISO_8601 for details) </summary>
        public static string ToReadableString_ISO8601(this DateTime dateTime) {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
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

    }

}