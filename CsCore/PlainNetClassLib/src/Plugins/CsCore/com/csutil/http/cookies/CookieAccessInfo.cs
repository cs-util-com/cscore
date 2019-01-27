namespace com.csutil.http.cookies {
    public class CookieAccessInfo {
        public string domain = null;
        public string path = null;
        /// <summary> By setting the secure flag, the cookie will only be transmitted if https is used </summary>
        public bool secure = false;
        public bool scriptAccessible = true;

        public CookieAccessInfo(string domain, string path) {
            this.domain = domain;
            this.path = path;
        }

        public CookieAccessInfo(string domain, string path, bool secure) {
            this.domain = domain;
            this.path = path;
            this.secure = secure;
        }

        public CookieAccessInfo(string domain, string path, bool secure, bool scriptAccessible) {
            this.domain = domain;
            this.path = path;
            this.secure = secure;
            this.scriptAccessible = scriptAccessible;
        }

        public CookieAccessInfo(Cookie cookie) {
            this.domain = cookie.domain;
            this.path = cookie.path;
            this.secure = cookie.secure;
            this.scriptAccessible = cookie.scriptAccessible;
        }
    }
}