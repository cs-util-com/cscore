using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Zio;

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

    public static class CookieContainerLoader {

        public static CookieContainer LoadFromFile(FileEntry sourceFile) {
            using (Stream stream = sourceFile.OpenForRead()) { return LoadFromStream(stream); }
        }
        public static CookieContainer LoadFromStream(Stream stream) {
            return (CookieContainer)new BinaryFormatter().Deserialize(stream);
        }

        public static void SaveToFile(this CookieContainer self, FileEntry targetFile) {
            using (Stream stream = targetFile.OpenOrCreateForWrite()) { SaveToStream(self, stream); }
        }
        public static void SaveToStream(this CookieContainer self, Stream stream) {
            new BinaryFormatter().Serialize(stream, self);
        }

    }

}