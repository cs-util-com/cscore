using System;

namespace com.csutil {

    public static class Numbers {

        public static bool HasNumberType(object o) { return HasWholeNumberType(o) || HasDecimalNumberType(o); }

        public static bool HasDecimalNumberType(object o) { return o is float || o is double || o is decimal; }

        public static bool IsDecimalNumberType(this Type t) { return t == typeof(float) || t == typeof(double) || t == typeof(decimal); }

        public static bool HasWholeNumberType(object o) {
            return o is sbyte || o is byte || o is short || o is ushort || o is int || o is uint || o is long || o is ulong;
        }

        public static bool IsWholeNumberType(this Type t) {
            return t == typeof(sbyte) || t == typeof(byte) || t == typeof(short) || t == typeof(ushort) || t == typeof(int)
                || t == typeof(uint) || t == typeof(long) || t == typeof(ulong);
        }

        /// <summary>
        /// Uses DataTable.Compute internally:
        /// See https://docs.microsoft.com/en-us/dotnet/api/system.data.datatable
        /// and https://docs.microsoft.com/en-us/dotnet/api/system.data.datacolumn.expression
        /// </summary>
        /// <param name="formula"> eg "(1 + 2.5 * 4) / 2" </param>
        public static double Calculate(string formula, bool logErrors = true) {
            try {
                formula = formula.Replace(",", "."); // Allow e.g 0.5 + 0,5
                return Convert.ToDouble(new System.Data.DataTable().Compute(formula, null));
            } catch (Exception e) {
                if (logErrors) { Log.e($"Failed to calculate formula '{formula}': {e.Message}", e); }
                throw;
            }
        }

    }

}