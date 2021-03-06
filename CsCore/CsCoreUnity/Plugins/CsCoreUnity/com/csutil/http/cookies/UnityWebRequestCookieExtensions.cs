﻿using com.csutil.http.cookies;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace com.csutil {

    public static class UnityWebRequestCookieExtensions {

        /// <summary> set all current cookies for each new request </summary>
        public static bool ApplyAllCookiesToRequest(this UnityWebRequest self) {
            return self.SetCookies(LoadStoredCookiesForUri(new Uri(self.url)));
        }

        private static List<Cookie> LoadStoredCookiesForUri(Uri uri) {
            try {
                CookieJar jar = IoC.inject.Get<CookieJar>(uri);
                if (jar == null) { return new List<Cookie>(); }
                var c = jar.GetCookies(new CookieAccessInfo(uri.Host, uri.AbsolutePath));
                if (c.IsNullOrEmpty() && uri.Scheme.Equals("https")) { c = jar.GetCookies(new CookieAccessInfo(uri.Host, uri.AbsolutePath, true)); }
                return c;
            }
            catch (Exception e) { Log.e(e); }
            return new List<Cookie>();
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
                List<string> cookies = GetResponseHeaders(self, "Set-Cookie");
                if (cookies.IsNullOrEmpty()) { return false; }
                foreach (var cookieString in cookies) { AddCookie(self, cookieString); }
                return true;
            }
            catch (Exception e) { Log.e(e); }
            return false;
        }

        private static List<string> GetResponseHeaders(UnityWebRequest self, string headerName) {
            List<string> cookies = new List<string>();
            cookies.Add(self.GetResponseHeader(headerName));
            return cookies;
        }

        private static bool AddCookie(UnityWebRequest self, string cookieString) {
            try {
                if (self == null || cookieString.IsNullOrEmpty()) { return false; }
                // from Response.cs:
                if (cookieString.IndexOf("domain=", StringComparison.CurrentCultureIgnoreCase) == -1) { cookieString += "; domain=" + self.url; }
                if (cookieString.IndexOf("path=", StringComparison.CurrentCultureIgnoreCase) == -1) { cookieString += "; path=" + self.url; }
                var cookieJar = IoC.inject.Get<CookieJar>(self);
                if (cookieJar == null) { return false; }
                return cookieJar.SetCookie(new Cookie(cookieString));
            }
            catch (Exception e) { Log.e(e); }
            return false;
        }

    }
}
