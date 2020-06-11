using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace com.csutil.http.cookies {
    public class Cookie {
        public string name = null;
        public string value = null;
        public DateTime expirationDate = DateTime.MaxValue;
        public string path = null;
        public string domain = null;
        public bool secure = false;
        public bool scriptAccessible = true;

        private static string cookiePattern = "\\s*([^=]+)(?:=((?:.|\\n)*))?";

        public Cookie(string cookieString) {
            string[] parts = cookieString.Split(';');
            foreach (string part in parts) {
                Match match = Regex.Match(part, cookiePattern);
                if (!match.Success) { throw new Exception("Could not parse cookie string: " + cookieString); }
                if (this.name == null) {
                    this.name = match.Groups[1].Value;
                    this.value = match.Groups[2].Value;
                    continue;
                }
                try {
                    var parsedKey = match.Groups[1].Value;
                    var parsedValue = match.Groups[2].Value;
                    switch (parsedKey.ToLowerInvariant()) {
                        case "httponly":
                            this.scriptAccessible = false;
                            break;
                        case "expires":
                            ParseTimestamp(parsedValue);
                            break;
                        case "path":
                            if (path != null && path != parsedValue) { Log.w("cookie[" + ToValueString() + "].path =" + path + " replaced with additional value: " + parsedValue); }
                            this.path = parsedValue;
                            break;
                        case "domain":
                            if (domain != null && domain != parsedValue) { Log.w("cookie[" + ToValueString() + "].domain=" + domain + " replaced with additional value: " + parsedValue); }
                            this.domain = parsedValue;
                            break;
                        case "secure":
                            this.secure = true;
                            break;
                        default:
                            // TODO: warn of unknown cookie setting?
                            break;
                    }
                } catch (Exception e) { Log.e("parsing error in cookie part=" + part + "; full cookieString=" + cookieString, e); }
            }
            if (this.domain.IsNullOrEmpty()) { Log.e(("domain is null or empty, full cookieString=" + cookieString)); }
        }

        private void ParseTimestamp(string timestampString) {
            try {
                expirationDate = DateTimeV2.ParseUtc(timestampString);
                if (expirationDate.ToUnixTimestampUtc() <= 0) {
                    if (expirationDate.ToUnixTimestampUtc() < 0) {
                        Log.e("cookie[" + ToString() + "]: will reject received expirationDate, dateString=" + timestampString);
                    }
                    expirationDate = DateTime.MaxValue;
                }
            } catch (Exception e) { Log.e("Error during parseTimestamp of: '" + timestampString + "'", e); }
        }

        public bool Matches(CookieAccessInfo accessInfo) {
            if (this.secure != accessInfo.secure) { return false; }
            return CollidesWith(accessInfo);
        }

        public bool CollidesWith(CookieAccessInfo accessInfo) {
            if (this.domain != null && accessInfo.domain == null) { return false; }
            if (this.path != null && accessInfo.path == null) { return false; }
            if (this.path != null && accessInfo.path != null && accessInfo.path.IndexOf(this.path) != 0) { return false; }
            if (this.domain == accessInfo.domain) {
                return true;
            } else if (this.domain != null && this.domain.Length >= 1 && this.domain[0] == '.') {
                int wildcard = accessInfo.domain.IndexOf(this.domain.Substring(1));
                if (wildcard == -1 || wildcard != accessInfo.domain.Length - this.domain.Length + 1) { return false; }
            } else if (this.domain != null) { return false; }
            return true;
        }

        public string ToValueString() { return name + "=" + value; }

        public override string ToString() {
            List<string> elements = new List<string>();
            elements.Add(this.name + "=" + this.value);
            if (this.expirationDate != DateTime.MaxValue) { elements.Add("expires=" + this.expirationDate.ToString()); }
            if (this.domain != null) { elements.Add("domain=" + this.domain); }
            if (this.path != null) { elements.Add("path=" + this.path); }
            if (this.secure) { elements.Add("secure"); }
            if (this.scriptAccessible == false) { elements.Add("httponly"); }
            return String.Join("; ", elements.ToArray());
        }
    }
}