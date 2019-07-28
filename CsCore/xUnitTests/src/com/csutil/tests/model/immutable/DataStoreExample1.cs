using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    public class DataStoreExample1 {

        public DataStoreExample1(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async void ExampleUsage1() {
            var t = Log.MethodEntered();

            var data = new MyAppState1();
            var thunk = Middlewares.NewThunkMiddleware<MyAppState1>();
            var recorder = new ReplayRecorder<MyAppState1>();
            var logging = Middlewares.NewLoggingMiddleware<MyAppState1>();
            var recMiddleware = recorder.CreateMiddleware();
            var undoable = new UndoRedoReducer<MyAppState1>();
            var undoReducer = undoable.wrap(MyReducers1.ReduceMyAppState1);
            var store = new DataStore<MyAppState1>(undoReducer, data, logging, thunk, recMiddleware);
            store.storeName = "Store 1";

            // Register a few listeners that listen to a subtree of the complete state tree:
            var firstContactWasModifiedCounter = 0;

            // listen to state changes of the first contact of the main user:
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

            Assert.Throws<Exception>(() => { store.Dispatch(new ActionOnUser.ChangeAge() { targetUser = "Peter", newAge = 100 }); });
            Assert.Throws<Exception>(() => { store.Dispatch(new ActionOnUser.ChangeName() { targetUser = "Peter", newName = "" }); });

            store.Dispatch(new ActionLogoutUser());
            Assert.Null(store.GetState().user);

            TestUndoAndRedo(store);
            await TestAsyncActions(store);

            await TestReplayRecorder(recorder, store);

            Log.MethodDone(t);
        }

        private static void TestUndoAndRedo(IDataStore<MyAppState1> store) {
            // there is nothing on the redo stack first:
            Assert.Throws<InvalidOperationException>(() => { store.Dispatch(new RedoAction<MyAppState1>()); });

            Assert.Null(store.GetState().user);
            store.Dispatch(new UndoAction<MyAppState1>()); // undo logout
            Assert.NotNull(store.GetState().user);

            store.Dispatch(new UndoAction<MyAppState1>()); // undo rename Tim => Peter
            Assert.Equal("Tim", store.GetState().user.contacts.First().name);

            store.Dispatch(new UndoAction<MyAppState1>()); // undo adding contact
            Assert.Null(store.GetState().user.contacts);

            store.Dispatch(new RedoAction<MyAppState1>()); // redo adding contact
            var contacts = store.GetState().user.contacts;
            Assert.Equal("Tim", contacts.First().name);

            // Add a new action:
            store.Dispatch(new ActionOnUser.AddContact() { targetUser = "Karl", newContact = new MyUser1(name: "Tim 2") });
            Assert.Equal(2, store.GetState().user.contacts.Count);

            // Again redo not possible at this point:
            Assert.Throws<InvalidOperationException>(() => { store.Dispatch(new RedoAction<MyAppState1>()); });

            store.Dispatch(new UndoAction<MyAppState1>()); // Undo adding additional user
            Assert.Same(contacts, store.GetState().user.contacts);

            // Using a type parameter that is not specified by the Undo Reducer does nothing:
            store.Dispatch(new UndoAction<string>());
            store.Dispatch(new RedoAction<string>());

        }

        private static async Task TestAsyncActions(IDataStore<MyAppState1> store) {
            Assert.Null(store.GetState().currentWeather);
            var a = store.Dispatch(NewAsyncGetWeatherAction());
            Assert.True(a is Task, "a=" + a.GetType());
            if (a is Task delayedTask) { await delayedTask; }
            Assert.NotEmpty(store.GetState().currentWeather);

            // Another asyn task example with an inline function:
            store.Dispatch(new ActionLogoutUser());
            Func<Task> asyncLoginTask = async () => {
                // Simulate that the login would talk to a server and take some time:
                await Task.Delay(100);
                store.Dispatch(new ActionLoginUser() { newLoggedInUser = new MyUser1("Karl") });
            };

            Assert.Null(store.GetState().user);
            await (store.Dispatch(asyncLoginTask) as Task);
            Assert.NotNull(store.GetState().user);
        }

        private async Task TestReplayRecorder(ReplayRecorder<MyAppState1> recorder, IDataStore<MyAppState1> store) {
            var finalState = store.GetState();
            recorder.ResetStore();
            Assert.Null(store.GetState().user);
            Assert.Null(store.GetState().currentWeather);
            await recorder.ReplayStore();
            Assert.NotEqual(0, recorder.recordedActionsCount);
            AssertEqualJson(finalState, store.GetState());
            await TestReplayRecorderOnNewStore(recorder, finalState);
        }

        private async Task TestReplayRecorderOnNewStore(ReplayRecorder<MyAppState1> recorder, MyAppState1 finalStateOfFirstStore) {
            var data2 = new MyAppState1();
            var logging = Middlewares.NewLoggingMiddleware<MyAppState1>();
            var recMiddleware = recorder.CreateMiddleware();
            var undoable = new UndoRedoReducer<MyAppState1>();
            var store2 = new DataStore<MyAppState1>(undoable.wrap(MyReducers1.ReduceMyAppState1), data2, logging, recMiddleware);
            store2.storeName = "Store 2";
            await recorder.ReplayStore();
            AssertEqualJson(finalStateOfFirstStore, store2.GetState());
        }

        private void AssertEqualJson<T>(T a, T b) {
            var expected = JsonWriter.GetWriter().Write(a);
            var actual = JsonWriter.GetWriter().Write(b);
            Assert.Equal(expected, actual);
        }

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
            return async (IDataStore<MyAppState1> store) => {
                var cityName = "New York";
                var foundLocations = await MetaWeatherLocationLookup.GetLocation(cityName);
                var report = await MetaWeatherReport.GetReport(foundLocations.First().woeid);
                var currentWeatherConditions = report.consolidated_weather.Map(r => r.weather_state_name);
                Log.d("currentWeatherConditions for " + cityName + ": " + currentWeatherConditions);
                store.Dispatch(new ActionSetWeather() { newWeather = currentWeatherConditions.ToList() });
            };
        }

        private static class MyReducers1 {

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
                    var contacts = ReduceContacts(user, isTargetUser, a, ref userChanged);
                    if (userChanged) { return new MyUser1(name, age, contacts); }
                }
                return user; // None of the fields changed, old user can be returned
            }

            private static ImmutableList<MyUser1> ReduceContacts(MyUser1 user, bool isTargetUser, ActionOnUser action, ref bool changed) {
                if (isTargetUser && action is ActionOnUser.AddContact a) {
                    changed = true;
                    return user.contacts.AddOrCreate(a.newContact);
                }
                return user.contacts.Mutate(action, ReduceEachContact, ref changed);
            }

            private static ImmutableList<MyUser1> ReduceEachContact(ImmutableList<MyUser1> previousState, object action) {
                return previousState.MutateEntries(action, ReduceUser);
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