using System.Threading.Tasks;
using com.csutil.http;
using Xunit;

namespace com.csutil.tests.http {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class HasInternetTests : IHasInternetListener {

        private bool hasInet;

        [Fact]
        public async Task TestInternetStateListenerOnce() {
            // First reset state of InternetStateManager to avoid side effects from other tests executed before this one:
            IoC.inject.RemoveAllInjectorsFor<InternetStateManager>();

            InternetStateManager.AddListener(this);
            Assert.True(await RestFactory.instance.HasInternet());
            Assert.True(await InternetStateManager.Instance(this).HasInetAsync);
            Assert.True(InternetStateManager.Instance(this).HasInet);
            Assert.True(hasInet);
            InternetStateManager.RemoveListener(this);
        }

        [Fact]
        public async Task TestInternetStateListener20Times() {
            for (int i = 0; i < 20; i++) {
                await TestInternetStateListenerOnce();
            }
        }

        Task IHasInternetListener.OnHasInternet(bool hasInet) {
            this.hasInet = hasInet;
            Assert.True(hasInet);
            return Task.FromResult(true);
        }

    }

}