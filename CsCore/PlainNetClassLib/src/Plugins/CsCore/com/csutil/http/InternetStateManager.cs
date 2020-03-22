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

        public static InternetStateManager Get(object caller) { return IoC.inject.GetOrAddSingleton<InternetStateManager>(caller); }

        public static async Task AddListener(IHasInternetListener l) {
            var m = Get(l);
            m.listeners.Add(l);
            await l.OnHasInternet(m.hasInet);
        }

        public bool hasInet { get; private set; } = false;
        public readonly List<IHasInternetListener> listeners = new List<IHasInternetListener>();
        public readonly CancellationTokenSource cancelToken = new CancellationTokenSource();

        public InternetStateManager() {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            TaskV2.RunRepeated(async () => {
                await HasInet(await RestFactory.instance.HasInternet());
                return true;
            }, delayInMsBetweenIterations: 3000, cancelToken: cancelToken.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public void Dispose() { cancelToken.Cancel(); }

        private async Task HasInet(bool hasInet) {
            this.hasInet = hasInet;
            foreach (var l in listeners) {
                try { await l.OnHasInternet(hasInet); } catch (Exception e) { Log.e(e); }
            }
        }

    }

}