using System;

namespace com.csutil {

    public static class EnumUtil {

        /// <summary> Will return the entry for the passed name if found, otherwise the passed in fallback entry </summary>
        public static T ParseOrFallback<T>(this T fallback, string entryName) where T : struct, Enum {
            if (Enum.TryParse(entryName, out T res)) { return res; } else { return fallback; }
        }

        public static string GetName<T>(this T entry) where T : Enum { return Enum.GetName(typeof(T), entry); }

    }

}