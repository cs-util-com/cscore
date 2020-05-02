using System;

namespace UltraLiteDB
{
    internal static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return str == null || str.Trim().Length == 0;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return str == null || str.Length == 0;
        }


    }
}