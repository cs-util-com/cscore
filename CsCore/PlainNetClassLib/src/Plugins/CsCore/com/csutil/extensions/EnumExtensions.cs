using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace com.csutil {

    public static class EnumUtil {

        public static T TryParse<T>(string entryName, T fallback) where T : struct
#if CSHARP_7_3 // Using Enum as a generic type constraint is only available in C# 7.3+
            , Enum 
#endif
        {
            EnforceMustBeEnum<T>();
            if (Enum.TryParse(entryName?.Replace(" ", ""), out T result)) { return result; } else { return fallback; }
        }

        public static bool TryParse<T>(string entryName, out T result) where T : struct
#if CSHARP_7_3 // Using Enum as a generic type constraint is only available in C# 7.3+
            , Enum 
#endif
        {
            EnforceMustBeEnum<T>();
            return Enum.TryParse(entryName?.Replace(" ", ""), out result);
        }

        /// <summary> Will return the entry for the passed name if found, otherwise the passed in fallback entry </summary>
        public static T TryParse<T>(this T fallback, string enumString) where T : struct
#if CSHARP_7_3 // Using Enum as a generic type constraint is only available in C# 7.3+
            , Enum 
#endif
        {
            EnforceMustBeEnum<T>();
            if (Enum.TryParse(enumString, out T res)) { return res; } else { return fallback; }
        }

        public static T Parse<T>(string enumString) where T : struct
#if CSHARP_7_3 // Using Enum as a generic type constraint is only available in C# 7.3+
            , Enum 
#endif
        {
            EnforceMustBeEnum<T>();
            if (enumString.IsNullOrEmpty()) { throw new ArgumentException("Cant parse null or emtpy string to enum " + typeof(T)); }
            return (T)Enum.Parse(typeof(T), enumString.Replace(" ", ""));
        }

        public static bool IsEnum<T>() { return typeof(T).IsEnum; }

        public static bool ContainsFlag<T>(this T self, T flag) where T : struct
#if CSHARP_7_3 // Using Enum as a generic type constraint is only available in C# 7.3+
            , Enum 
#endif 
        {
            EnforceMustBeEnum<T>();
            return ((Enum)(object)self).HasFlag((Enum)(object)flag);
        }

        public static string GetEntryName<T>(this T entry) where T : struct
#if CSHARP_7_3 // Using Enum as a generic type constraint is only available in C# 7.3+
            , Enum 
#endif
        {
            EnforceMustBeEnum<T>();
            return Enum.GetName(typeof(T), entry);
        }

        [Conditional("DEBUG")]
        private static void EnforceMustBeEnum<T>([CallerMemberName] string methodName = null) where T : struct {
            if (!IsEnum<T>()) { throw Log.e($"Method '{methodName}' must only be used with enums!"); }
        }

    }

}