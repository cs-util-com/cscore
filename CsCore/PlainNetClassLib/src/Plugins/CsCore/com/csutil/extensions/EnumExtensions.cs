using System;
using System.Collections.Generic;
using System.Text;

namespace com.csutil {

    public static class EnumHelper {

        public static T TryParse<T>(string enumString, T defaultValue) where T : struct {
            if (Enum.TryParse(enumString.Replace(" ", ""), out T result)) { return result; } else { return defaultValue; }
        }

        public static T Parse<T>(string enumString) { return (T)Enum.Parse(typeof(T), enumString); }

        public static bool IsEnum<T>() { return typeof(T).IsEnum; }

    }

}
