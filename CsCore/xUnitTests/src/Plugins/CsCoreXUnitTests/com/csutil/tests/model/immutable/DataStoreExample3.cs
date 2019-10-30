using System;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.json;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    /// <summary> 
    /// This is a simple redux datastore example that mutates an immutable datamodel using actions that 
    /// are dispatched to the datastore. Related articles about server outbox & offline flows: 
    //  - https://hackernoon.com/introducing-redux-offline-offline-first-architecture-for-progressive-web-applications-and-react-68c5167ecfe0
    //  - https://medium.com/@ianovenden/adding-offline-support-to-redux-ac8eb8873035
    /// </summary>
    public class DataStoreExample3 {

        public DataStoreExample3(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            var t = Log.MethodEntered("DataStoreExample3.ExampleUsage1");

            // Add a thunk middleware to allow dispatching async actions:
            var thunkMiddleware = Middlewares.NewThunkMiddleware<MyAppState1>();

            // aDD A logging middleware to log all dispatched actions:
            var loggingMiddleware = Middlewares.NewLoggingMiddleware<MyAppState1>();

            var serverOutboxHandler = new ServerOutboxHandler<MyAppState1>();
            // To allow undo redo on the full store wrap the main reducer with the undo reducer:
            var outboxReducer = serverOutboxHandler.Wrap(MyReducers1.ReduceMyAppState1);
            var initialState = new MyAppState1(); // the initial immutable state
            var store = new DataStore<MyAppState1>(outboxReducer, initialState, loggingMiddleware, thunkMiddleware);
            IoC.inject.SetSingleton(store);
            store.storeName = "Store 3";

            { // Do a login which is an async server action that cant be cached optimistically and wont work offline:
                Func<Task> asyncLoginTask = async () => {
                    await TaskV2.Delay(100);
                    store.Dispatch(new ActionUserLoggedIn() { newLoggedInUser = new MyUser1("a1@b.com") });
                };
                await (store.Dispatch(asyncLoginTask) as Task);
            }
            { // Change the email a first time:
                var a = new ActionOnUser.ChangeEmail() { targetEmail = "a1@b.com", newEmail = "a2@b.com" };
                store.Dispatch(a);
                Assert.Equal(a, store.GetState().serverOutbox.serverActions.First());
                Assert.False(store.GetState().user.emailConfirmed);
            }
            { // Change the email a second time:
                var a = new ActionOnUser.ChangeEmail() { targetEmail = "a2@b.com", newEmail = "a3@b.com" };
                store.Dispatch(a);
                Assert.Equal(a, store.GetState().serverOutbox.serverActions.Last());
            }

            Assert.Equal(2, store.GetState().serverOutbox.serverActions.Count);
            await store.SyncWithServer(store.GetState().serverOutbox.serverActions.First());
            Assert.Single(store.GetState().serverOutbox.serverActions);
            await store.SyncWithServer(store.GetState().serverOutbox.serverActions.First());
            Assert.Empty(store.GetState().serverOutbox.serverActions);
            Assert.True(store.GetState().user.emailConfirmed);

            { // Simulate a server task that has a timeout:
                var a = new ActionOnUser.ChangeEmail() { targetEmail = "a3@b.com", newEmail = "a4@b.com", simulateOneTimeout = true };
                store.Dispatch(a);
                Assert.Single(store.GetState().serverOutbox.serverActions);
                Assert.False(store.GetState().user.emailConfirmed);
                await store.SyncWithServer(a);
                Assert.Empty(store.GetState().serverOutbox.serverActions);
                Assert.Equal(2, a.sentToServerCounter);
                Assert.True(store.GetState().user.emailConfirmed);
            }
            { // Simulate the server rejecting an email change:
                var a = new ActionOnUser.ChangeEmail() { targetEmail = "a4@b.com", newEmail = "a5@b.com", simulateError = true };
                store.Dispatch(a);
                await store.SyncWithServer(a);
                Assert.Empty(store.GetState().serverOutbox.serverActions);
                Assert.Equal("a4@b.com", store.GetState().user.email);
                Assert.True(store.GetState().user.emailConfirmed);
            }
            { // Test persisting and restoring the full store and continue with the pending server requests:
                store.Dispatch(new ActionOnUser.ChangeEmail() { targetEmail = "a4@b.com", newEmail = "a5@b.com" });
                store.Dispatch(new ActionOnUser.ChangeEmail() { targetEmail = "a5@b.com", newEmail = "a6@b.com" });
                Assert.Equal(2, store.GetState().serverOutbox.serverActions.Count);
                Assert.False(store.GetState().user.emailConfirmed);
                Assert.Equal("a6@b.com", store.GetState().user.email);

                // Simulate persisiting the store to disk and back into memory:
                string persistedStateJson = TypedJsonHelper.NewTypedJsonWriter().Write(store.GetState());

                store.Destroy(); // Destroy the old store before loading the state again into an new store
                var data2 = TypedJsonHelper.NewTypedJsonReader().Read<MyAppState1>(persistedStateJson);
                var store2 = new DataStore<MyAppState1>(outboxReducer, data2, loggingMiddleware, thunkMiddleware);
                IoC.inject.SetSingleton(store2, overrideExisting: true);
                store2.storeName = "Store 3 (2)";

                Assert.Equal(2, store2.GetState().serverOutbox.serverActions.Count);
                Assert.False(store2.GetState().user.emailConfirmed);
                Assert.Equal("a6@b.com", store2.GetState().user.email);

                // Sync the pending server tasks one after another:
                foreach (var serverAction in store2.GetState().serverOutbox.serverActions) {
                    await store2.SyncWithServer(serverAction);
                }
                Assert.True(store2.GetState().user.emailConfirmed);
                Assert.NotNull(store2.GetState().serverOutbox);
                Assert.Empty(store2.GetState().serverOutbox.serverActions);
            }

        }

        private class MyAppState1 : HasServerOutbox {
            public readonly MyUser1 user;
            public ServerOutbox serverOutbox { get; set; }
            public MyAppState1(MyUser1 user = null) { this.user = user; }

        }

        private class MyUser1 {
            public readonly string email;
            public readonly bool emailConfirmed;

            public MyUser1(string email, bool emailConfirmed = false) {
                this.email = email;
                this.emailConfirmed = emailConfirmed;
            }
        }

        #region example actions

        private class ActionUserLoggedIn {
            public MyUser1 newLoggedInUser;
        }

        private class ActionLogoutUser {
        }

        private class ActionOnUser {

            public string targetEmail;
            public bool IsTargetUser(MyUser1 user) { return user.email == targetEmail; }

            public class ChangeEmail : ActionOnUser, ServerAction {
                public string newEmail;
                public bool simulateError = false;
                public bool simulateOneTimeout = false;
                public int sentToServerCounter = 0;

                public async Task<ServerActionResult> SendToServer() {
                    var t = Log.MethodEntered();
                    await TaskV2.Delay(10); // communication with server would happen here
                    sentToServerCounter++;
                    if (simulateOneTimeout) { simulateOneTimeout = false; return ServerActionResult.RETRY; }
                    if (simulateError) { return ServerActionResult.FAIL; }
                    // After communicating with the server an additional store update might be needed:
                    var serverConfirmedEmail = true; // <- this answer would come from the server
                    var store = IoC.inject.Get<DataStore<MyAppState1>>(this);
                    store.Dispatch(new ActionOnUser.EmailConfirmed() {
                        targetEmail = newEmail,
                        isEmailConfirmed = serverConfirmedEmail
                    });
                    return ServerActionResult.SUCCESS;
                }

                public Task RollbackLocalChanges(ServerActionResult reasonForRollback) {
                    Log.MethodEntered("reasonForRollback=" + reasonForRollback);
                    // Here the server probably has to be asked for the old email to rollback to and 
                    // then a local update would have to be done
                    if (this.newEmail == "a5@b.com") { // See testflow above
                        var emailOnServer = "a4@b.com"; // Received from the server
                        var store = IoC.inject.Get<DataStore<MyAppState1>>(this);
                        store.Dispatch(new ActionOnUser.RollbackLocalUser() {
                            targetEmail = newEmail,
                            emailOnServer = emailOnServer,
                            isEmailConfirmed = true
                        });
                    }
                    return Task.FromResult(true);
                }

            }

            public class EmailConfirmed : ActionOnUser { public bool isEmailConfirmed; }

            public class RollbackLocalUser : ActionOnUser {
                public string emailOnServer;
                public bool isEmailConfirmed;
            }
        }

        #endregion // of example actions

        private static class MyReducers1 { // The reducers to modify the immutable datamodel:

            // The most outer reducer is public to be passed into the store:
            public static MyAppState1 ReduceMyAppState1(MyAppState1 previousState, object action) {
                bool changed = false;
                var newUser = previousState.user.Mutate(action, ReduceUser, ref changed);
                if (changed) { return new MyAppState1(newUser); }
                return previousState;
            }

            private static MyUser1 ReduceUser(MyUser1 user, object action) {
                if (action is ActionLogoutUser) { return null; }
                if (action is ActionUserLoggedIn a1) { return a1.newLoggedInUser; }
                if (action is ActionOnUser a) {
                    bool userChanged = false; // Will be set to true by the .Mutate method if any field changes:
                    var isTargetUser = a.IsTargetUser(user);
                    var email = user.email.Mutate(isTargetUser, a, ReduceUserEmail, ref userChanged);
                    var emailConfirmed = user.emailConfirmed.Mutate(isTargetUser, a, ReduceEmailConfirmed, ref userChanged);
                    if (userChanged) { return new MyUser1(email, emailConfirmed); }
                }
                return user; // None of the fields changed, old user can be returned
            }

            private static string ReduceUserEmail(string oldEmail, object action) {
                if (action is ActionOnUser.ChangeEmail a) { return a.newEmail; }
                if (action is ActionOnUser.RollbackLocalUser r) { return r.emailOnServer; }
                return oldEmail;
            }

            private static bool ReduceEmailConfirmed(bool emailConfirmed, object action) {
                if (action is ActionOnUser.ChangeEmail) { return false; }
                if (action is ActionOnUser.EmailConfirmed a2) { return a2.isEmailConfirmed; }
                if (action is ActionOnUser.RollbackLocalUser r) { return r.isEmailConfirmed; }
                return emailConfirmed;
            }

        }

    }

}