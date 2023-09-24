using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using com.csutil.http;
using com.csutil.http.cookies;
using Xunit;

namespace com.csutil.integrationTests.http {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class RestCookieTests {

        [Fact]
        public async Task ExampleUsage() {

            using var cleanup = new CleanupHelper();
            var injector1 = IoC.inject.SetSingleton(new CookieContainer());
            cleanup.AddInjectorCleanup<CookieContainer>(injector1);

            var cookieJar = IoC.inject.Get<CookieContainer>(null, false);
            Assert.NotNull(cookieJar);

            var uri = new Uri("https://httpbin.org/cookies");

            cookieJar.Add(uri, new System.Net.Cookie("coo1", "cooVal1"));
            cookieJar.Add(uri, new System.Net.Cookie("coo2", "cooVal2"));
            await SendCookiesAndAssertIncluded(uri);
            // CookieContainer will be used every time & will include all cookies received by the backend:
            await SendCookiesAndAssertIncluded(uri);
            
            IoC.inject.Get<IRestFactory>(this).Dispose(); // Cleanup
        }

        [Fact]
        public async Task TestCookieContainer() {
            using var cleanup = new CleanupHelper();
            var injector = IoC.inject.SetSingleton(new CookieContainer());
            cleanup.AddInjectorCleanup<CookieContainer>(injector);

            var cookieJar = IoC.inject.Get<CookieContainer>(null, false);
            Assert.NotNull(cookieJar);

            var uri = new Uri("https://httpbin.org/cookies");

            cookieJar.Add(uri, new System.Net.Cookie("coo1", "cooVal1"));
            cookieJar.Add(uri, new System.Net.Cookie("coo2", "cooVal2"));
            await SendCookiesAndAssertIncluded(uri);

            IoC.inject.RemoveAllInjectorsFor<CookieContainer>();
            Assert.Null(IoC.inject.Get<CookieContainer>(null, false));

            var dir = EnvironmentV2.instance.GetNewInMemorySystem();
            var cookiesFile = dir.GetChild("CookieContainer.bin");
            cookieJar.SaveToFile(cookiesFile); // Save it and then load it again
            IoC.inject.SetSingleton(CookieContainerLoader.LoadFromFile(cookiesFile));
            // Reset the rest factory now that a new CookieContainer should be used:
            IoC.inject.SetSingleton<IRestFactory>(new RestFactory(), true);
            await SendCookiesAndAssertIncluded(uri);
            
            IoC.inject.Get<IRestFactory>(this).Dispose(); // Cleanup
        }

        [Fact]
        public async Task TestCookieJar() { // Deprecated in favor of System.Net.CookieContainer
            using var cleanup = new CleanupHelper();
            var injector = IoC.inject.SetSingleton<CookieJar>(new InMemoryCookieJar());
            cleanup.AddInjectorCleanup<CookieContainer>(injector);

            var cookieJar = IoC.inject.Get<CookieJar>(null, false);
            Assert.NotNull(cookieJar);

            var uri = new Uri("https://httpbin.org/cookies");

            cookieJar.SetCookie(csutil.http.cookies.Cookie.NewCookie("coo1", "cooVal1", uri.Host));
            cookieJar.SetCookie(csutil.http.cookies.Cookie.NewCookie("coo2", "cooVal2", uri.Host));
            var resp = await uri.SendGET().GetResult<HttpbinCookieResp>();
            Assert.Contains(resp.cookies, x => x.Key == "coo1" && x.Value == "cooVal1");
            Assert.Contains(resp.cookies, x => x.Key == "coo2" && x.Value == "cooVal2");
            
            IoC.inject.Get<IRestFactory>(this).Dispose(); // Cleanup
        }

        private static async Task SendCookiesAndAssertIncluded(Uri uri) {
            var resp = await uri.SendGET().GetResult<HttpbinCookieResp>();
            Assert.Contains(resp.cookies, x => x.Key == "coo1" && x.Value == "cooVal1");
            Assert.Contains(resp.cookies, x => x.Key == "coo2" && x.Value == "cooVal2");
        }

        private class HttpbinCookieResp {
            public Dictionary<string, string> cookies { get; set; }
        }

        /// <summary> This implementation does not persist any cookies, so the callbacks are all NOOP </summary>
        private class InMemoryCookieJar : CookieJar {
            protected override void LoadAllCookies() { }
            protected override void PersistAllCookies(Dictionary<string, List<csutil.http.cookies.Cookie>> all) { }
            protected override void DeleteAllCookies() { }
        }

    }

}