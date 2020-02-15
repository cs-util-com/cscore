using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace com.csutil.model.immutable {

    public class ServerOutboxHandler<T> where T : HasServerOutbox {

        public StateReducer<T> Wrap(StateReducer<T> wrappedReducer) {
            return (T present, object action) => {
                var newPresent = wrappedReducer(present, action);
                if (action is ServerAction sa) {
                    var serverActions = MutateServerActions(present, sa);
                    newPresent.serverOutbox = new ServerOutbox() { serverActions = serverActions };
                } else if (action is RemoveServerAction fa) {
                    var serverActions = present.serverOutbox.serverActions.Remove(fa.finishedServerAction);
                    newPresent.serverOutbox = new ServerOutbox() { serverActions = serverActions };
                } else {
                    newPresent.serverOutbox = present.serverOutbox;
                }
                return newPresent;
            };
        }

        private ImmutableList<ServerAction> MutateServerActions(T store, ServerAction a) {
            if (store.serverOutbox == null) { return ImmutableList.Create<ServerAction>(a); }
            AssertV2.IsFalse(store.serverOutbox.serverActions.Contains(a), "Action " + a + " already in the action list!");
            return store.serverOutbox.serverActions.Add(a);
        }

    }

    public static class ServerOutboxSyncExtensions {

        public static async Task<ServerActionResult> SyncWithServer<T>(this IDataStore<T> self,
                            ServerAction pendingServerAction, int maxRetries = 25, int retryCounter = 0) where T : HasServerOutbox {
            var result = await pendingServerAction.SyncWithServer(maxRetries, retryCounter);
            self.Dispatch(new RemoveServerAction { finishedServerAction = pendingServerAction, finishResult = result });
            return result;
        }

        /// <summary> After calling this the store has to be manually informed with a RemoveServerAction that the action can be removed! </summary>
        public static async Task<ServerActionResult> SyncWithServer(this ServerAction self, int maxRetries = 25, int retryCounter = 0) {
            var result = ServerActionResult.FAIL;
            try { result = await self.SendToServer(); } catch (System.Exception e) { Log.e(e); }
            if (result == ServerActionResult.RETRY && retryCounter < maxRetries) {
                var delayInMs = Math.Pow(2, retryCounter);  // Delay via exponential backoff before next retry
                await TaskV2.Delay((int)delayInMs);
                return await self.SyncWithServer(maxRetries, retryCounter + 1);
            }
            if (result == ServerActionResult.RETRY || result == ServerActionResult.FAIL) {
                await self.RollbackLocalChanges(result);
            }
            return result;
        }

    }

    public class RemoveServerAction {
        public ServerAction finishedServerAction;
        public ServerActionResult finishResult;
    }

    public interface HasServerOutbox {
        /// <summary> Will contain the list of unsynced server actions </summary>
        ServerOutbox serverOutbox { get; set; }
    }

    public class ServerOutbox { public ImmutableList<ServerAction> serverActions; }

    public enum ServerActionResult {
        /// <summary> Used when the the server processed the action successfully </summary>
        SUCCESS,
        /// <summary> Used when the action fails temporary, e.g. the server could not be reached or had a 5xx error </summary>
        RETRY,
        /// <summary> Used when the action fails permanently, e.g the server returned a 4xx error </summary>
        FAIL,
    }

    public interface ServerAction {
        Task<ServerActionResult> SendToServer();
        Task RollbackLocalChanges(ServerActionResult reasonForRollback);
    }

}