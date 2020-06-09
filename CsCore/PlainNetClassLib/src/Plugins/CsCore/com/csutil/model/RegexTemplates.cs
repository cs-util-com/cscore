using System.Linq;
using System.Text.RegularExpressions;

namespace com.csutil.model {

    /// <summary> Related links: https://www.debuggex.com and http://regexlib.com </summary>
    public static class RegexTemplates {

        public const string EMAIL_ADDRESS = "^[_A-Za-z0-9-\\+]+(\\.[_A-Za-z0-9-]+)*@"
            + "[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$";
        public const string ZIP_CODE = "^\\d{5}$";
        public const string USERNAME = "^[A-Za-z0-9_-]{3,16}$";
        public const string URL = "^(https?:\\/\\/)?([\\da-z\\.-]+)\\.([a-z\\.]{2,6})([\\/\\w \\.-]*)*\\/?$";
        public const string PHONE_NR = "^\\+?[\\d\\s]{3,}$";

        /// <summary> Matches if string is not emtpy and not only whitespaces </summary>
        public const string NON_EMPTY_STRING = "^(?!\\s*$).+";
        public const string NUMBER_INTEGER = "^-{0,1}\\d+$";
        public const string COLOR_HEX_RGB = "^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";
        public const string COLOR_HEX_RGBA = "^#([A-Fa-f0-9]{8}|[A-Fa-f0-9]{4})$";

        public const string DATEddmmyyyy = "^([1-9]|0[1-9]|[12][0-9]|3[01])\\D([1-9]|0[1-9]|1[012])\\D(19[0-9][0-9]|20[0-9][0-9])$";
        public const string DATEmmddyyyy = "(0?[1-9]|1[012])/(0?[1-9]|[12][0-9]|3[01])/((19|20)\\d\\d)";
        public const string TIME_12h = "(1[012]|[1-9]):[0-5][0-9](\\s)?(?i)(am|pm)";
        public const string TIME_24h = "([01]?[0-9]|2[0-3]):[0-5][0-9]";

        public const string HAS_UPPERCASE = ".*[A-Z].*";
        public const string HAS_LOWERCASE = ".*[a-z].*";
        public const string HAS_NUMBER = @".*\d.*";
        public const string HAS_SPECIAL_CHAR = @".*\W.*";

    }

    public static class RegexUtil {

        private static Regex camelCaseSplitter = new Regex(@"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))");

        public static string SplitCamelCaseString(string camelCaseString) {
            return camelCaseSplitter.Replace(camelCaseString, " $1").ToFirstCharUpperCase();
        }

        /// <summary> Combines multiple regex via AND </summary>
        public static string CombineViaOr(params string[] regex) {
            if (regex.Length == 1) { return regex.Single(); }
            return regex.Map(r => "(" + r + ")").ToStringV2("^", "$", "|");
        }

        /// <summary> Combines multiple regex via AND </summary>
        public static string CombineViaAnd(params string[] regex) {
            if (regex.Length == 1) { return regex.Single(); }
            // See https://stackoverflow.com/a/470602/165106
            return regex.Reduce("^", (res, r) => res + "(?=.*" + r + ")") + ".*$";
        }

        /// <summary> Generates a regex that enforces a minimum input length </summary>
        public static string MinLengthRegex(int minChars) { return "^.{" + minChars + ",}$"; }

        /// <summary> Generates a regex that enforces a minimum and maximum input length </summary>
        public static string MinAndMaxLengthRegex(int minChars, int maxChars) {
            return "^.{" + minChars + "," + maxChars + "}$";
        }

    }

}