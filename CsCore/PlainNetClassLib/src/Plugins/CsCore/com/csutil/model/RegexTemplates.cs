namespace com.csutil.model {
    // Related links: 
    // - http://regexlib.com/ 
    // - https://www.debuggex.com/
    public static class RegexTemplates {

        public const string EMAIL_ADDRESS = "^[_A-Za-z0-9-\\+]+(\\.[_A-Za-z0-9-]+)*@"
            + "[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$";
        public const string ZIP_CODE = "^\\d{5}$";
        public const string USERNAME = "^[a-z0-9_-]{3,16}$";
        public const string URL = "^(https?:\\/\\/)?([\\da-z\\.-]+)\\.([a-z\\.]{2,6})([\\/\\w \\.-]*)*\\/?$";
        public const string IP = "^([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])$";
        public const string PHONE_NR = "^\\+?[\\d\\s]{3,}$";


        public const string NUMBER = "^[-+]?[0-9]*\\.?[0-9]+$";
        public const string NUMBER_INTEGER = "^-{0,1}\\d+$";
        public const string NUMBER_DECIMAL = "^-{0,1}\\d*\\.{0,1}\\d+$";
        public const string COLOR_HEX_RGB = "^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";
        public const string COLOR_HEX_RGBA = "^#([A-Fa-f0-9]{8}|[A-Fa-f0-9]{4})$";


        public const string DATEddmmyyyy = "^([1-9]|0[1-9]|[12][0-9]|3[01])\\D([1-9]|0[1-9]|1[012])\\D(19[0-9][0-9]|20[0-9][0-9])$";
        public const string DATEmmddyyyy = "(0?[1-9]|1[012])/(0?[1-9]|[12][0-9]|3[01])/((19|20)\\d\\d)";
        public const string TIME_12h = "(1[012]|[1-9]):[0-5][0-9](\\s)?(?i)(am|pm)";
        public const string TIME_24h = "([01]?[0-9]|2[0-3]):[0-5][0-9]";

    }
}