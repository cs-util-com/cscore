using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    /// <summary> This is a simple redux datastore example that mutates an immutable datamodel using actions that are dispatched to the datastore </summary>
    public class DataStoreExample1 {

        public DataStoreExample1(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {
            var t = Log.MethodEntered("DataStoreExample1.ExampleUsage1");

            var data = new MyAppState1(); // the initial immutable state
            var store = new DataStore<MyAppState1>(MyReducers1.ReduceMyAppState1, data);

            // Register a listener that listens to a subtree of the complete state tree:
            var firstContactWasModifiedCounter = 0;
            // Listen to state changes of the first contact of the main user:
            store.AddStateChangeListener(state => state.user?.contacts?.FirstOrDefault(), (firstContact) => {
                firstContactWasModifiedCounter++;
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

            store.Dispatch(new ActionLogoutUser());
            Assert.Null(store.GetState().user);

            Log.MethodDone(t);
        }

        private class MyAppState1 {
            public readonly MyUser1 user;
            public MyAppState1(MyUser1 user = null) { this.user = user; }
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

        #region example actions

        private class ActionLoginUser { public MyUser1 newLoggedInUser; }
        private class ActionLogoutUser { }

        private class ActionOnUser {
            public string targetUser;
            public bool IsTargetUser(MyUser1 user) { return user.name == targetUser; }

            public class ChangeName : ActionOnUser { public string newName; }
            public class ChangeAge : ActionOnUser { public int newAge; }
            public class AddContact : ActionOnUser { public MyUser1 newContact; }

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
                if (action is ActionLoginUser a1) { return a1.newLoggedInUser; }
                if (action is ActionOnUser a) {
                    bool userChanged = false; // Will be set to true by the .Mutate method if any field changes:
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
                if (action is ActionOnUser.ChangeName a) { return a.newName; }
                return oldName;
            }

            private static int ReduceUserAge(int oldAge, object action) {
                if (action is ActionOnUser.ChangeAge a) { return a.newAge; }
                return oldAge;
            }

        }

    }

}