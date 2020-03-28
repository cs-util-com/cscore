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

    public class InternetStateManager : IDisposable {

        public static InternetStateManager Instance(object caller) { return IoC.inject.GetOrAddSingleton<InternetStateManager>(caller); }

        public static void AddListener(IHasInternetListener l) { Instance(l).listeners.Add(l); }
        public static bool RemoveListener(IHasInternetListener l) { return Instance(l).listeners.Remove(l); }

        public bool HasInet { get; private set; } = false;
        public Task<bool> HasInetAsync { get; private set; }

        public readonly List<IHasInternetListener> listeners = new List<IHasInternetListener>();
        public readonly CancellationTokenSource cancelToken = new CancellationTokenSource();

        public InternetStateManager() {
            HasInetAsync = RunInternetCheck();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RunInternetCheckLoop(HasInetAsync);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public void Dispose() { cancelToken.Cancel(); }

        private async Task RunInternetCheckLoop(Task firstInetCheck) {
            await firstInetCheck;
            await TaskV2.RunRepeated(async () => {
                HasInetAsync = RunInternetCheck();
                await HasInetAsync;
                return true;
            }, delayInMsBetweenIterations: 3000, cancelToken: cancelToken.Token);
        }

        private async Task<bool> RunInternetCheck() {
            await SetHasInet(await RestFactory.instance.HasInternet());
            return HasInet;
        }

        private async Task SetHasInet(bool hasInet) {
            this.HasInet = hasInet;
            foreach (var l in listeners) {
                try { await l.OnHasInternet(hasInet); } catch (Exception e) { Log.e(e); }
            }
        }

    }

}