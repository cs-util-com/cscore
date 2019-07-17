using System;
using System.Collections.Immutable;
using System.Linq;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    public class DataStoreExample1 {

        public DataStoreExample1(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {
            var t = Log.MethodEntered();

            var data = new MyAppState1();
            var store = new DataStore<MyAppState1>(MyReducers1.ReduceMyAppState1, data, loggingMiddleware);

            // Register a few listeners that listen to a subtree of the complete state tree:
            var firstContactWasModifiedCounter = 0;
            store.AddStateChangeListener(state => state.user?.contacts?.FirstOrDefault(), (firstContact) => {
                firstContactWasModifiedCounter++;
                Assert.True(firstContactWasModifiedCounter < 4, "counter=" + firstContactWasModifiedCounter);
                if (firstContactWasModifiedCounter == 1) { // 1st event when the contact is added:
                    Assert.Equal("Tim", firstContact.name);
                } else if (firstContactWasModifiedCounter == 2) { // 2nd event when the contacts name is changed:
                    Assert.Equal("Peter", firstContact.name);
                } else { // 3rd event when the user is logged out at the end:
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

            Log.MethodDone(t);
        }

        public class MyAppState1 {
            public readonly MyUser1 user;
            public MyAppState1(MyUser1 user = null) { this.user = user; }
        }

        public class MyUser1 {
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

        private Func<Dispatcher, Dispatcher> loggingMiddleware(DataStore<MyAppState1> store) {
            Log.MethodEntered("store=" + store);
            return (Dispatcher innerDispatcher) => {
                Dispatcher loggingDispatcher = (action) => {
                    var previousState = store.GetState();
                    var returnedAction = innerDispatcher(action);
                    var newState = store.GetState();
                    if (Object.Equals(previousState, newState)) {
                        Log.e("The action  " + action + " was not handled by any of the reducers!");
                    } else {
                        Log.d(asJson("previousState", previousState), asJson("" + action.GetType(), action), asJson("newState", newState));
                    }
                    return returnedAction;
                };
                return loggingDispatcher;
            };
        }

        private static string asJson(string varName, object result) { return varName + "=" + JsonWriter.AsPrettyString(result); }

        public static class MyReducers1 {

            public static MyAppState1 ReduceMyAppState1(MyAppState1 previousState, object action) {
                bool changed = false;
                var newUser = previousState.user.Mutate(action, ReduceUser, ref changed);
                if (changed) { return new MyAppState1(newUser); }
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