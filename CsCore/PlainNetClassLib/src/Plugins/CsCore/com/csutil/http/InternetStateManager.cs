using com.csutil.http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace com.csutil {

    public interface IHasInternetListener {

        /// <summary> This method will be called roughly every second with the current internet state </summary>
        /// <param name="hasInet"> true if currently internet is available </param>
        /// <returns> A task that will be avaited before the next listener is informed </returns>
        Task OnHasInternet(bool hasInet);

    }

    public class InternetStateManager : IDisposable, IsDisposable {

        public static InternetStateManager Instance(object caller) { return IoC.inject.GetOrAddSingleton<InternetStateManager>(caller); }

        public static void AddListener(IHasInternetListener l) {
            Instance(l).listeners.Add(l);
        }
        public static bool RemoveListener(IHasInternetListener l) {
            return Instance(l).listeners.Remove(l);
        }

        public bool HasInet { get; private set; } = false;
        public Task<bool> HasInetAsync { get; private set; }

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;

        public readonly ISet<IHasInternetListener> listeners = new HashSet<IHasInternetListener>();
        public readonly CancellationTokenSource cancelToken = new CancellationTokenSource();

        public InternetStateManager() {
            HasInetAsync = RunInternetCheck();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RunInternetCheckLoop(HasInetAsync);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public void Dispose() {
            IsDisposed = DisposeState.DisposingStarted;
            cancelToken.Cancel();
            if (IoC.inject.Get<InternetStateManager>(this) == this) { IoC.inject.RemoveAllInjectorsFor<InternetStateManager>(); }
            IsDisposed = DisposeState.Disposed;
        }

        private async Task RunInternetCheckLoop(Task firstInetCheck) {
            await firstInetCheck;
            await TaskV2.RunRepeated(async () => {
                HasInetAsync = RunInternetCheck();
                await HasInetAsync;
                return true;
            }, delayInMsBetweenIterations: 3000, cancelToken: cancelToken.Token);
        }

        protected virtual async Task<bool> RunInternetCheck() {
            var newState = await RestFactory.instance.HasInternet();
            await SetHasInet(newState);
            AssertV2.AreEqual(HasInet, newState);
            return HasInet;
        }

        private async Task SetHasInet(bool hasInet) {
            this.HasInet = hasInet;
            foreach (var l in listeners) {
                try {
                    await l.OnHasInternet(hasInet);
                } catch (Exception e) { Log.e(e); }
            }
        }

    }

}