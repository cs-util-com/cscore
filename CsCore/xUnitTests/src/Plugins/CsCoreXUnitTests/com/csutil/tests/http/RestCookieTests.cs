using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using com.csutil.http.cookies;
using Xunit;

namespace com.csutil.tests.http {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class RestCookieTests {

        [Fact]
        public async Task TestCookies() {
            IoC.inject.SetSingleton<CookieJar>(new InMemoryCookieJar());
            var cookieJar = IoC.inject.Get<CookieJar>(null, false);
            Assert.NotNull(cookieJar);

            var uri = new Uri("https://httpbin.org/cookies");

            cookieJar.SetCookie(csutil.http.cookies.Cookie.NewCookie("coo1", "cooVal1", uri.Host));
            cookieJar.SetCookie(csutil.http.cookies.Cookie.NewCookie("coo2", "cooVal2", uri.Host));
            var resp = await uri.SendGET().GetResult<HttpbinCookieResp>();
            Assert.Contains(resp.cookies, x => x.Key == "coo1" && x.Value == "cooVal1");
            Assert.Contains(resp.cookies, x => x.Key == "coo2" && x.Value == "cooVal2");
        }

        [Fact]
        public async Task TestCookies2() {
            IoC.inject.SetSingleton(new CookieContainer());
            var cookieJar = IoC.inject.Get<CookieContainer>(null, false);
            Assert.NotNull(cookieJar);

            var uri = new Uri("https://httpbin.org/cookies");

            cookieJar.Add(uri, new System.Net.Cookie("coo1", "cooVal1"));
            cookieJar.Add(uri, new System.Net.Cookie("coo2", "cooVal2"));
            await SendCookiesAndAssertIncluded(uri);
            // CookieContainer will be used every time & will include all cookies received by the backend:
            await SendCookiesAndAssertIncluded(uri);
        }

        private static async Task SendCookiesAndAssertIncluded(Uri uri) {
            var resp = await uri.SendGET().GetResult<HttpbinCookieResp>();
            Assert.Contains(resp.cookies, x => x.Key == "coo1" && x.Value == "cooVal1");
            Assert.Contains(resp.cookies, x => x.Key == "coo2" && x.Value == "cooVal2");
        }

        private class HttpbinCookieResp { public Dictionary<string, string> cookies { get; set; } }

        /// <summary> This implementation does not persist any cookies, so the callbacks are all NOOP </summary>
        private class InMemoryCookieJar : CookieJar {
            protected override void LoadAllCookies() { }
            protected override void PersistAllCookies(Dictionary<string, List<csutil.http.cookies.Cookie>> all) { }
            protected override void DeleteAllCookies() { }
        }

    }

}