using com.csutil.http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace com.csutil {

    public interface IHasInternetListener {

        /// <summary> This method will be called roughly every second with the current internet state </summary>
        /// <param name="hasInet"> true if currently internet is available </param>
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
            TaskV2.RunRepeated(async () => {
                await HasInet(await RestFactory.instance.HasInternet());
                return true;
            }, delayInMsBetweenIterations: 1000, cancelToken.Token);
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