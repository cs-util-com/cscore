using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace com.csutil.http.cookies {

    public abstract class CookieJar { // Ported version of https://github.com/bmeck/node-cookiejar
        private const string version = "v2";
        private const string cookiesStringPattern = "[:](?=\\s*[a-zA-Z0-9_\\-]+\\s*[=])";
        private const string boundary = "\n!!::!!\n";

        private object cookieJarLock = new object();
        private Dictionary<string, List<Cookie>> cookies = new Dictionary<string, List<Cookie>>();
        public ContentsChangedDelegate ContentsChanged;

        public delegate void ContentsChangedDelegate();

        private CookieJar() {
            LoadCompleteCookieDictionary();
        }

        internal abstract void LoadCompleteCookieDictionary();

        public override string ToString() {
            var s = "CookieJar:";
            foreach (var cookie in cookies) {
                s += ("\n   > cookie.key=" + cookie.Key);
                foreach (var valueParams in cookie.Value) { s += ("\n       > cookie.value=" + valueParams); }
            }
            return s;
        }

        public void ClearCookies(bool deleteCachedCookiesFile = true) {
            Log.w("ClearCookies (deleteCachedCookiesFile=" + deleteCachedCookiesFile + ")");
            lock (cookieJarLock) {
                cookies.Clear();
                if (deleteCachedCookiesFile) { DeleteAllCookies(); }
                InformContentChangeListener();
            }
        }

        internal abstract void DeleteAllCookies();

        private void InformContentChangeListener() { if (ContentsChanged != null) { ContentsChanged(); } }

        public bool SetCookie(Cookie cookie, bool saveToCookieFile = true) {
            lock (cookieJarLock) {
                if (cookie.expirationDate.ToUnixTimestampUtc() <= 0) { }
                bool receivedCookieExpired = cookie.expirationDate.IsBefore(DateTimeV2.UtcNow);
                if (cookies.ContainsKey(cookie.name)) {
                    for (int index = 0; index < cookies[cookie.name].Count; ++index) {
                        Cookie collidableCookie = cookies[cookie.name][index];
                        if (collidableCookie.CollidesWith(new CookieAccessInfo(cookie))) {
                            if (cookie.value.IsNullOrEmpty() && !collidableCookie.value.IsNullOrEmpty()) {
                                Log.e((collidableCookie + " replaced by " + (receivedCookieExpired ? "expired " : " ") + cookie));
                            }
                            if (receivedCookieExpired) {
                                cookies[cookie.name].RemoveAt(index);
                                if (cookies[cookie.name].Count == 0) {
                                    cookies.Remove(cookie.name);
                                    if (saveToCookieFile) { SaveAllCookies(); }
                                }
                                return false;
                            } else {
                                cookies[cookie.name][index] = cookie;
                                if (saveToCookieFile) { SaveAllCookies(); }
                                return true;
                            }
                        }
                    }
                    if (receivedCookieExpired) { Log.w("Expired cookie will not be added! cookie=" + cookie); return false; }
                    cookies[cookie.name].Add(cookie);
                    if (saveToCookieFile) { SaveAllCookies(); }
                    return true;
                }
                if (receivedCookieExpired) { Log.w("Expired cookie will not be added! cookie=" + cookie); return false; }
                AssertV2.IsFalse(cookies.ContainsKey(cookie.name), "cookies[cookie.name] was not null");
                if (!cookies.ContainsKey(cookie.name)) { cookies[cookie.name] = new List<Cookie>(); }
                cookies[cookie.name].Add(cookie);
                ShowDebugWarningIfMoreThanOneCookieWithSameName(cookie.name);
                if (saveToCookieFile) { SaveAllCookies(); }
                return true;
            }
        }

        private void ShowDebugWarningIfMoreThanOneCookieWithSameName(string cookieName) {
            if (cookies[cookieName].Count > 1) {
                Log.d(cookies[cookieName].Count + " cookies found with name " + cookieName);
                for (int i = 0; i < cookies[cookieName].Count; ++i) {
                    Log.d("    > entry " + i + ": " + cookies[cookieName][i]);
                }
            }
        }

        private void SaveAllCookies() {
            var savingWorked = saveCompleteCookieDictionary();
            if (!savingWorked) { Log.e(("Could not save cookies file")); }
            InformContentChangeListener();
        }

        internal abstract bool saveCompleteCookieDictionary();

        // TODO: figure out a way to respect the scriptAccessible flag and supress cookies being
        //       returned that should not be.  The issue is that at some point, within this
        //       library, we need to send all the correct cookies back in the request.  Right now
        //       there's no way to add all cookies (regardless of script accessibility) to the
        //       request without exposing cookies that should not be script accessible.

        public Cookie GetCookie(string name, CookieAccessInfo accessInfo) {
            if (!cookies.ContainsKey(name)) { return null; }
            for (int index = 0; index < cookies[name].Count; ++index) {
                Cookie cookie = cookies[name][index];
                if (cookie.Matches(accessInfo)) {
                    AssertV2.IsTrue(cookie.expirationDate.ToUnixTimestampUtc() > 0, "cookie.expirationDate.toUnixTimestamp()=" + cookie.expirationDate.ToUnixTimestampUtc());
                    if (cookie.expirationDate.IsAfter(DateTimeV2.UtcNow)) {
                        return cookie;
                    } else {
                        Log.w("Matching but expired cookie found, expirationDate=" + cookie.expirationDate.ToReadableString() + "; cookie=" + cookie);
                    }
                }
            }
            return null;
        }

        [Obsolete("use GetCookies(CookieAccessInfo accessInfo) instead", true)]
        public List<Cookie> GetCookies(string url) {
            foreach (KeyValuePair<string, List<Cookie>> k in cookies) { Log.d(k.Key + ": " + k.Value.ToString()); }
            if (cookies.ContainsKey(url)) { return cookies[url]; }
            return new List<Cookie>();
        }

        public bool HasCookie(string cookieName) { return !cookies.IsNullOrEmpty() && cookies.ContainsKey(cookieName); }

        public List<Cookie> GetCookies(CookieAccessInfo accessInfo) {
            List<Cookie> result = new List<Cookie>();
            foreach (string cookieName in cookies.Keys) {
                Cookie cookie = this.GetCookie(cookieName, accessInfo);
                if (cookie != null) { result.Add(cookie); }
            }
            return result;
        }

        public void SetCookies(Cookie[] cookieObjects) {
            for (var i = 0; i < cookieObjects.Length; ++i) { SetCookie(cookieObjects[i]); }
        }

        public void SetCookies(string cookiesString) {
            Match match = Regex.Match(cookiesString, cookiesStringPattern);
            if (!match.Success) { throw new Exception("Could not parse cookies string: " + cookiesString); }
            for (int index = 0; index < match.Groups.Count; ++index) { SetCookie(new Cookie(match.Groups[index].Value)); }
        }
    }

}