using System;
using System.Collections.Generic;
using System.Net;

namespace com.csutil.http {

    public static class CookieContainerExtensions {

        public static void LoadFromCookieJarIntoCookieContainer(this cookies.CookieJar source, Uri uri, CookieContainer target) {
            var cookies = source?.GetCookies(new cookies.CookieAccessInfo(uri.Host, uri.AbsolutePath));
            if (!cookies.IsNullOrEmpty()) {
                foreach (var c in cookies) {
                    target.Add(uri, new Cookie(c.name, c.value));
                }
            }
        }

        public static List<cookies.Cookie> GetCookiesForCookieJar(this CookieContainer self, Uri uri) {
            var cookies = self.GetCookies(uri);
            var converted = new List<cookies.Cookie>();
            for (int i = 0; i < cookies.Count; i++) {
                var c = cookies[i];
                converted.Add(http.cookies.Cookie.NewCookie(c.Name, c.Value, c.Domain));
            }
            return converted;
        }

    }

}