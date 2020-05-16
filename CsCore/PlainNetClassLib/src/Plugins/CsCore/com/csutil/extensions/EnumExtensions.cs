using System;

namespace com.csutil {

    public static class EnumUtil {

        public static T TryParse<T>(string entryName, T fallback) where T : struct, Enum {
            if (Enum.TryParse(entryName?.Replace(" ", ""), out T result)) { return result; } else { return fallback; }
        }

        public static bool TryParse<T>(string entryName, out T result) where T : struct, Enum {
            return Enum.TryParse(entryName?.Replace(" ", ""), out result);
        }

        /// <summary> Will return the entry for the passed name if found, otherwise the passed in fallback entry </summary>
        public static T TryParse<T>(this T fallback, string entryName) where T : struct, Enum {
            if (Enum.TryParse(entryName, out T res)) { return res; } else { return fallback; }
        }

        public static T Parse<T>(string enumString) {
            if (enumString.IsNullOrEmpty()) { throw new ArgumentException("Cant parse null or emtpy string to enum " + typeof(T)); }
            return (T)Enum.Parse(typeof(T), enumString);
        }

        public static bool IsEnum<T>() { return typeof(T).IsEnum; }

        internal static bool ContainsFlag<T>(this T self, T flag) where T : Enum {
            return self.HasFlag(flag);
        }

        public static string GetName<T>(this T entry) where T : Enum { return Enum.GetName(typeof(T), entry); }

    }

}
