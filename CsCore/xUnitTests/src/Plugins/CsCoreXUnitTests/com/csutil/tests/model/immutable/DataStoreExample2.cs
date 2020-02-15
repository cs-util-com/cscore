using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    /// <summary> 
    /// This is a more complex redux datastore example that uses features like: 
    /// - UNDO/REDO by using a higher order reducer around the normal main reducer
    /// - A thunk middleware to enable dispatching async actions
    /// - A recorder middleware to record and replay all actions of a store
    /// </summary>
    public class DataStoreExample2 {

        public DataStoreExample2(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage2() {
            var t = Log.MethodEntered("DataStoreExample2.ExampleUsage2");

            // Add a thunk middleware to allow dispatching async actions:
            var thunkMiddleware = Middlewares.NewThunkMiddleware<MyAppState1>();

            // aDD A logging middleware to log all dispatched actions:
            var loggingMiddleware = Middlewares.NewLoggingMiddleware<MyAppState1>();

            // Add a recorder middleware to enable hot reload by replaying previously recorded actions:
            var recorder = new ReplayRecorder<MyAppState1>();
            var recMiddleware = recorder.CreateMiddleware();

            var undoable = new UndoRedoReducer<MyAppState1>();
            // To allow undo redo on the full store wrap the main reducer with the undo reducer:
            var undoReducer = undoable.wrap(MyReducers1.ReduceMyAppState1);

            var data = new MyAppState1(); // the initial immutable state
            var store = new DataStore<MyAppState1>(undoReducer, data, loggingMiddleware, recMiddleware, thunkMiddleware);
            store.storeName = "Store 1";

            TestNormalDispatchOfActions(store);

            TestUndoAndRedo(store);

            await TestAsyncActions(store);

            await TestReplayRecorder(recorder, store);

            Log.MethodDone(t);
        }

        private static void TestNormalDispatchOfActions(IDataStore<MyAppState1> store) {
            var t = Log.MethodEntered("TestNormalDispatchOfActions");

            // Register a listener that listens to a subtree of the complete state tree:
            var firstContactWasModifiedCounter = 0;
            // Listen to state changes of the first contact of the main user:
            store.AddStateChangeListener(state => state.user?.contacts?.FirstOrDefault(), (firstContact) => {
                firstContactWasModifiedCounter++;
                if (firstContactWasModifiedCounter == 1) { // 1st event when the contact is added:
                    Assert.Equal("Tim", firstContact.name);
                } else if (firstContactWasModifiedCounter == 2) { // 2nd event when the contacts name is changed:
                    Assert.Equal("Peter", firstContact.name);
                } else if (firstContactWasModifiedCounter == 3) { // 3rd event when the user is logged out at the end:
                    Assert.Null(firstContact);
                }
            });

            store.Dispatch(new ActionLoginUser() { newLoggedInUser = new MyUser1("Karl") });
            Assert.NotNull(store.GetState().user);

            store.Dispatch(new ActionOnUser.ChangeAge() { targetUser = "Karl", newAge = 99 });
            Assert.Equal(99, store.GetState().user.age);

            store.Dispatch(new ActionOnUser.AddContact() { targetUser = "Karl", newContact = new MyUser1(name: "Tim") });
            Assert.Equal("Tim", store.GetState().user.contacts.First().name);
            Assert.Equal(1, firstContactWasModifiedCounter);

            // Change name of Tim to Peter:
            store.Dispatch(new ActionOnUser.ChangeName() { targetUser = "Tim", newName = "Peter" });
            Assert.Equal("Peter", store.GetState().user.contacts.First().name);
            Assert.Equal(2, firstContactWasModifiedCounter);

            // Test that the reducers throw errors for invalid actions being dispatched (max age is 99 and name must not be emtpy):
            Assert.Throws<Exception>(() => { store.Dispatch(new ActionOnUser.ChangeAge() { targetUser = "Peter", newAge = 100 }); });
            Assert.Throws<Exception>(() => { store.Dispatch(new ActionOnUser.ChangeName() { targetUser = "Peter", newName = "" }); });

            store.Dispatch(new ActionLogoutUser());
            Assert.Null(store.GetState().user);

            Log.MethodDone(t);
        }

        private static void TestUndoAndRedo(IDataStore<MyAppState1> store) {
            var t = Log.MethodEntered("TestUndoAndRedo");

            // There is nothing on the redo stack first:
            Assert.Throws<InvalidOperationException>(() => { store.Dispatch(new RedoAction<MyAppState1>()); });

            Assert.Null(store.GetState().user);
            store.Dispatch(new UndoAction<MyAppState1>()); // undo logout
            Assert.NotNull(store.GetState().user); // User logged in again in the store

            Assert.Equal("Peter", store.GetState().user.contacts.First().name);
            store.Dispatch(new UndoAction<MyAppState1>()); // undo that Tim was renamed to Peter
            Assert.Equal("Tim", store.GetState().user.contacts.First().name);

            store.Dispatch(new UndoAction<MyAppState1>()); // undo adding first contact
            Assert.Null(store.GetState().user.contacts); // Now the contacts are emtpy

            store.Dispatch(new RedoAction<MyAppState1>()); // redo adding first contact
            var contacts = store.GetState().user.contacts; // Now the contacts contain 1 user again
            Assert.Equal("Tim", contacts.First().name);

            // Add a new action:
            store.Dispatch(new ActionOnUser.AddContact() { targetUser = "Karl", newContact = new MyUser1(name: "Tim 2") });
            Assert.Equal(2, store.GetState().user.contacts.Count);

            // Again redo not possible at this point because the redo stack was cleared when a new action was dispatched:
            Assert.Throws<InvalidOperationException>(() => { store.Dispatch(new RedoAction<MyAppState1>()); });

            store.Dispatch(new UndoAction<MyAppState1>()); // Undo adding additional user
            Assert.Same(contacts, store.GetState().user.contacts);

            // Using a type parameter that is not specified by the Undo Reducer does nothing:
            store.Dispatch(new UndoAction<string>()); // Does nothing to the state
            store.Dispatch(new RedoAction<string>()); // Does nothing to the state

            Log.MethodDone(t);
        }

        private static async Task TestAsyncActions(IDataStore<MyAppState1> store) {
            var t = Log.MethodEntered("TestAsyncActions");

            Assert.Null(store.GetState().currentWeather);
            // Create an async action and dispatch it so that it is executed by the thunk middleware:
            var a = store.Dispatch(NewAsyncGetWeatherAction());
            Assert.True(a is Task, "a=" + a.GetType());
            if (a is Task delayedTask) { await delayedTask; }
            Assert.NotEmpty(store.GetState().currentWeather);

            store.Dispatch(new ActionLogoutUser());
            Assert.Null(store.GetState().user);

            // Another asyn task example with an inline lambda function:
            Func<Task> asyncLoginTask = async () => {
                await TaskV2.Delay(100); // Simulate that the login would talk to a server and take some time
                // Here the user would be logged into the server and returned to the client to store it:
                store.Dispatch(new ActionLoginUser() { newLoggedInUser = new MyUser1("Karl") });
            };
            // Since the async action uses Func<Task> the returned object can be awaited on:
            await (store.Dispatch(asyncLoginTask) as Task);
            Assert.NotNull(store.GetState().user);

            Log.MethodDone(t);
        }

        private async Task TestReplayRecorder(ReplayRecorder<MyAppState1> recorder, IDataStore<MyAppState1> store) {
            var t = Log.MethodEntered("TestReplayRecorder");

            // First remember the final state of the store:
            var finalState = store.GetState();
            // Then reset the store so that it is in its initial state again:
            recorder.ResetStore();
            Assert.Null(store.GetState().user);
            Assert.Null(store.GetState().currentWeather);

            // Calling ReplayStore will replay all actions stored by the recorder so that the final state is restored:
            await recorder.ReplayStore();
            Assert.NotEqual(0, recorder.recordedActionsCount);
            AssertEqualJson(finalState, store.GetState());

            // The recorder middleware can also replay the actions into a second store:
            await TestReplayRecorderOnNewStore(recorder, finalState);

            Log.MethodDone(t);
        }

        private async Task TestReplayRecorderOnNewStore(ReplayRecorder<MyAppState1> recorder, MyAppState1 finalStateOfFirstStore) {
            var t = Log.MethodEntered("TestReplayRecorderOnNewStore");

            // Connect the recorder to the new store:
            var recMiddleware = recorder.CreateMiddleware();
            var undoable = new UndoRedoReducer<MyAppState1>();
            var logging = Middlewares.NewLoggingMiddleware<MyAppState1>();

            var data2 = new MyAppState1();
            var store2 = new DataStore<MyAppState1>(undoable.wrap(MyReducers1.ReduceMyAppState1), data2, logging, recMiddleware);
            store2.storeName = "Store 2";

            // Replaying the recorder will now fill the second store with the same actions:
            await recorder.ReplayStore();
            AssertEqualJson(finalStateOfFirstStore, store2.GetState());

            Log.MethodDone(t);
        }

        private void AssertEqualJson<T>(T a, T b) { // Compare 2 objects based on their json
            var expected = JsonWriter.GetWriter().Write(a);
            Assert.False(expected.IsNullOrEmpty());
            var actual = JsonWriter.GetWriter().Write(b);
            Assert.Equal(expected, actual);
        }

        #region example datamodel

        private class MyAppState1 {
            public readonly MyUser1 user;
            public readonly List<string> currentWeather;
            public MyAppState1(MyUser1 user = null, List<string> currentWeather = null) {
                this.user = user;
                this.currentWeather = currentWeather;
            }
        }

        private class MyUser1 {
            public readonly string name;
            public readonly int age;
            public readonly ImmutableList<MyUser1> contacts;

            public MyUser1(string name, int age = 0, ImmutableList<MyUser1> contacts = null) {
                this.name = name;
                this.age = age;
                this.contacts = contacts;
            }
        }

        #endregion // of example datamodel

        #region example actions

        private class ActionLogoutUser { }

        private class ActionLoginUser { public MyUser1 newLoggedInUser; }

        private class ActionOnUser {
            public string targetUser;
            public bool IsTargetUser(MyUser1 user) { return user.name == targetUser; }

            public class ChangeName : ActionOnUser { public string newName; }
            public class ChangeAge : ActionOnUser { public int newAge; }
            public class AddContact : ActionOnUser { public MyUser1 newContact; }

        }

        private class ActionSetWeather { public List<string> newWeather; }

        private static Func<IDataStore<MyAppState1>, Task> NewAsyncGetWeatherAction() {
            // The created method is executed by the thunk middlewhere when its dispatched in a store:
            return async (IDataStore<MyAppState1> store) => {
                var loadedWeather = await DownloadWeatherFor("New York");
                store.Dispatch(new ActionSetWeather() { newWeather = loadedWeather });
            };
        }

        private static async Task<List<string>> DownloadWeatherFor(string cityName) {
            var foundLocations = await MetaWeatherLocationLookup.GetLocation(cityName);
            if (foundLocations == null) { // Assume test currently has no internet so simulate:
                return new List<string>() { "Rain", "Cloudy" };
            }
            var report = await MetaWeatherReport.GetReport(foundLocations.First().woeid);
            var currentWeatherConditions = report.consolidated_weather.Map(r => r.weather_state_name);
            Log.d("currentWeatherConditions for " + cityName + ": " + currentWeatherConditions);
            return currentWeatherConditions.ToList();
        }

        #endregion // of example actions

        private static class MyReducers1 { // The reducers to modify the immutable datamodel:

            // The most outer reducer is public to be passed into the store:
            public static MyAppState1 ReduceMyAppState1(MyAppState1 previousState, object action) {
                bool changed = false;
                if (action is ResetStoreAction) { return new MyAppState1(); }
                var newWeather = previousState.currentWeather.Mutate(action, ReduceWeather, ref changed);
                var newUser = previousState.user.Mutate(action, ReduceUser, ref changed);
                if (changed) { return new MyAppState1(newUser, newWeather); }
                return previousState;
            }

            private static List<string> ReduceWeather(List<string> previousState, object action) {
                if (action is ActionSetWeather a) { return a.newWeather; }
                return previousState;
            }

            private static MyUser1 ReduceUser(MyUser1 user, object action) {
                if (action is ActionLogoutUser) { return null; }
                if (action is ActionLoginUser a1) { return a1.newLoggedInUser; }
                if (action is ActionOnUser a) {
                    bool userChanged = false;
                    var isTargetUser = a.IsTargetUser(user);
                    var name = user.name.Mutate(isTargetUser, a, ReduceUserName, ref userChanged);
                    var age = user.age.Mutate(isTargetUser, a, ReduceUserAge, ref userChanged);
                    var contacts = user.MutateField(user.contacts, a, ReduceContacts, ref userChanged);
                    if (userChanged) { return new MyUser1(name, age, contacts); }
                }
                return user; // None of the fields changed, old user can be returned
            }

            private static ImmutableList<MyUser1> ReduceContacts(MyUser1 user, ImmutableList<MyUser1> contacts, object action) {
                contacts = contacts.MutateEntries(action, ReduceUser);
                if (action is ActionOnUser.AddContact a && a.IsTargetUser(user)) {
                    return ImmutableExtensions.AddOrCreate<MyUser1>(contacts, (MyUser1)a.newContact);
                }
                return contacts;
            }

            private static string ReduceUserName(string oldName, object action) {
                if (action is ActionOnUser.ChangeName a) {
                    if (a.newName.IsNullOrEmpty()) { throw Log.e("New name invalid"); }
                    return a.newName;
                }
                return oldName;
            }

            private static int ReduceUserAge(int oldAge, object action) {
                if (action is ActionOnUser.ChangeAge a) {
                    if (a.newAge < 0 || a.newAge > 99) { throw Log.e("New age invalid: " + a.newAge); }
                    return a.newAge;
                }
                return oldAge;
            }

        }

    }

}