using System;

namespace com.csutil {
    public static class DateTimeExtensions {
        /// <summary> sortable, short, hard to read wrong, can be used in pathes. read also https://stackoverflow.com/a/15952652/165106 </summary>
        public static string ToReadableString(this DateTime self) {
            return self.ToUniversalTime().ToString("yyyy-MM-dd_HH.mm");
        }

        public static DateTime NewDateTimeFromUnixTimestamp(this DateTime ignored, long unixTimeInMs) {
            AssertV2.IsTrue(unixTimeInMs > 0, "NewDateTimeFromUnixTimestamp: unixTimeInMs was " + unixTimeInMs);
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dtDateTime.AddMilliseconds(unixTimeInMs);
        }

        public static long ToUnixTimestamp(this DateTime self) {
            var zero = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return Convert.ToInt64(Math.Truncate((self.ToUniversalTime().Subtract(zero)).TotalMilliseconds));
        }

        public static bool IsBefore(this DateTime self, DateTime other) {
            return self.ToUnixTimestamp() < other.ToUnixTimestamp();
        }

        public static bool IsAfter(this DateTime self, DateTime other) {
            return self.ToUnixTimestamp() > other.ToUnixTimestamp();
        }

    }
}