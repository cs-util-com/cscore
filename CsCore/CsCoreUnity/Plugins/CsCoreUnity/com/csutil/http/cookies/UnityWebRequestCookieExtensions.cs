using com.csutil.http.cookies;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace com.csutil {

    public static class UnityWebRequestCookieExtensions {

        /// <summary> set all current cookies for each new request </summary>
        public static bool ApplyAllCookiesToRequest(this UnityWebRequest self) {
            Uri uri = new Uri(self.url);
            try {
                // If available use the CookieContainer to apply set cookies to the request:
                var container = IoC.inject.Get<System.Net.CookieContainer>(uri);
                if (container != null) {
                    self.SetRequestHeader("Cookie", container.GetCookieHeader(uri));
                    return true;
                }
                // Else use the CookieJar (if available):
                CookieJar jar = IoC.inject.Get<CookieJar>(uri);
                if (jar == null) { return true; }
                var allCookies = jar.GetCookies(new CookieAccessInfo(uri.Host, uri.AbsolutePath));
                if (allCookies.IsNullOrEmpty() && uri.Scheme.Equals("https")) {
                    allCookies = jar.GetCookies(new CookieAccessInfo(uri.Host, uri.AbsolutePath, true));
                }
                return self.SetCookies(allCookies);
            }
            catch (Exception e) { Log.e(e); }
            return false;
        }

        /// <summary> can be used to manually set a specific set of cookies, normally not needed since cookies are applied automatically by applyAllCookies()! </summary>
        private static bool SetCookies(this UnityWebRequest self, List<Cookie> cookieList) {
            if (cookieList.IsNullOrEmpty()) { return false; }
            string allCookies = "";
            bool allCookiesSetCorrectly = true;
            foreach (var cookie in cookieList) {
                try {
                    var newcookies = allCookies + cookie.name + "=" + cookie.value + ";";
                    self.SetRequestHeader("Cookie", newcookies);
                    allCookies = newcookies;
                }
                catch (Exception e) {
                    Log.e("Cant set invalid cookie: " + cookie.name + "=" + cookie.value, e);
                    allCookiesSetCorrectly = false;
                }
            }
            return allCookiesSetCorrectly;
        }

        public static bool SaveAllNewCookiesFromResponse(this UnityWebRequest self) {
            try {
                string cookieHeader = self.GetResponseHeader("Set-Cookie");
                if (cookieHeader.IsNullOrEmpty()) { return false; }

                var cookieJar = IoC.inject.Get<CookieJar>(self, createIfNull: false);
                if (cookieJar != null) { SetCookieHeader(self, cookieHeader, cookieJar); }

                var container = IoC.inject.Get<System.Net.CookieContainer>(self, createIfNull: false);
                if (container != null) { container.SetCookies(self.uri, cookieHeader); }

                return cookieJar != null || container != null;
            }
            catch (Exception e) { Log.e(e); }
            return false;
        }

        private static bool SetCookieHeader(UnityWebRequest self, string cookieHeader, CookieJar cookieJar) {
            try {
                if (self == null || cookieHeader.IsNullOrEmpty()) { return false; }
                if (cookieHeader.IndexOf("domain=", StringComparison.CurrentCultureIgnoreCase) == -1) { cookieHeader += "; domain=" + self.uri.Host; }
                if (cookieHeader.IndexOf("path=", StringComparison.CurrentCultureIgnoreCase) == -1) { cookieHeader += "; path=" + self.uri.LocalPath; }
                return cookieJar.SetCookie(new Cookie(cookieHeader));
            }
            catch (Exception e) { Log.e(e); }
            return false;
        }

    }
}
