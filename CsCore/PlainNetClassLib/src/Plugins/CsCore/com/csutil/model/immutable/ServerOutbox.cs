using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace com.csutil.model.immutable {

    // TODO compare to images:
    // - https://hackernoon.com/introducing-redux-offline-offline-first-architecture-for-progressive-web-applications-and-react-68c5167ecfe0
    // - https://medium.com/@ianovenden/adding-offline-support-to-redux-ac8eb8873035
    public class ServerOutboxHandler<T> where T : HasServerOutbox {

        public StateReducer<T> Wrap(StateReducer<T> wrappedReducer) {
            return (T present, object action) => {
                var newPresent = wrappedReducer(present, action);
                if (action is ServerAction sa) {
                    var serverActions = MutateServerActions(present, sa);
                    newPresent.serverOutbox = new ServerOutbox(serverActions);
                } else if (action is RemoveFinishedServerAction fa) {
                    var serverActions = present.serverOutbox.serverActions.Remove(fa.finishedServerAction);
                    newPresent.serverOutbox = new ServerOutbox(serverActions);
                } else {
                    newPresent.serverOutbox = present.serverOutbox;
                }
                return newPresent;
            };
        }

        private ImmutableList<ServerAction> MutateServerActions(T store, ServerAction a) {
            if (store.serverOutbox == null) { return ImmutableList.Create<ServerAction>(a); }
            return store.serverOutbox.serverActions.Add(a);
        }

    }

    public static class ServerOutboxSyncExtensions {

        [Obsolete("TODO fix signature")]
        public static async Task SyncWithServer<T>(this IDataStore<T> self) where T : HasServerOutbox {
            var outbox = self.GetState().serverOutbox;
            await self.SyncWithServer(outbox.serverActions.First());
        }

        public static async Task<ServerActionResult> SyncWithServer<T>(this IDataStore<T> self,
                    ServerAction pendingServerAction, int maxRetries = 25, int retryCounter = 0) where T : HasServerOutbox {
            var result = ServerActionResult.FAIL;
            try { result = await pendingServerAction.SendToServer(); } catch (System.Exception e) { Log.e(e); }
            if (result == ServerActionResult.RETRY && maxRetries > 0) {
                var delayInMs = Math.Pow(2, retryCounter);  // Delay via exponential backoff before next retry
                await TaskV2.Delay((int)delayInMs);
                return await SyncWithServer(self, pendingServerAction, maxRetries - 1, retryCounter + 1);
            }
            if (result == ServerActionResult.RETRY || result == ServerActionResult.FAIL) { await pendingServerAction.RollbackLocalChanges(result); }
            self.Dispatch(new RemoveFinishedServerAction { finishedServerAction = pendingServerAction, finishResult = result });
            return result;
        }

    }

    public class RemoveFinishedServerAction {
        public ServerAction finishedServerAction;
        public ServerActionResult finishResult;
    }

    public interface HasServerOutbox {
        /// <summary> Will contain the list of unsynced server actions </summary>
        ServerOutbox serverOutbox { get; set; }
    }

    public class ServerOutbox {
        public readonly ImmutableList<ServerAction> serverActions;
        internal ServerOutbox(ImmutableList<ServerAction> serverActions) { this.serverActions = serverActions; }
    }

    public enum ServerActionResult {
        /// <summary> Used when the the server processed the action successfully </summary>
        SUCCESS,
        /// <summary> Used when the server could not be reached or had a 5xx error </summary>
        RETRY,
        /// <summary> Used when the server returned a 4xx error so the action will never succeed </summary>
        FAIL,
    }

    public interface ServerAction {
        Task<ServerActionResult> SendToServer();
        Task RollbackLocalChanges(ServerActionResult reasonForRollback);
    }

}