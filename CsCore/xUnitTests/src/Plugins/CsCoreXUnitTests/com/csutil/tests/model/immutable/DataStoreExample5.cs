using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    /// <summary> This is another redux store example with an larger immutable datamodel containing common field types to 
    /// show how a collection of Reducers looks like for typical real word scenarios  </summary>
    public class DataStoreExample5 {

        public DataStoreExample5(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {
            var t = Log.MethodEntered("DataStoreExample1.ExampleUsage1");

            // Some initial state of the model (eg loaded from file when the app is started) is restored and put into the store:
            MyUser1 carl = new MyUser1(GuidV2.NewGuid(), "Carl", 99, null, MyUser1.MyEnum.State1);
            var data = new MyAppState1(ImmutableDictionary<Guid, MyUser1>.Empty.Add(carl.id, carl), null);

            var store = new DataStore<MyAppState1>(MyReducers1.ReduceMyAppState1, data);

            var usersChangedCounter = 0;
            store.AddStateChangeListener(state => state.users, (changedUsers) => {
                usersChangedCounter++;
            }, triggerInstantToInit: false);

            ActionAddSomeId a1 = new ActionAddSomeId() { someId = GuidV2.NewGuid() };
            store.Dispatch(a1);
            Assert.Equal(0, usersChangedCounter); // no change happened in the users
            Assert.Equal(a1.someId, store.GetState().someUuids.Value.Single());

            MyUser1 carlsFriend = new MyUser1(GuidV2.NewGuid(), "Carls Friend", 50, null, MyUser1.MyEnum.State1);
            store.Dispatch(new ActionOnUser.AddContact() { targetUser = carl.id, newContact = carlsFriend });
            Assert.Equal(1, usersChangedCounter);
            Assert.Equal(carlsFriend, store.GetState().users[carlsFriend.id]);
            Assert.Contains(carlsFriend.id, store.GetState().users[carl.id].contacts);

            store.Dispatch(new ActionOnUser.ChangeName() { targetUser = carl.id, newName = "Karl" });
            Assert.Equal(2, usersChangedCounter);
            Assert.Equal("Karl", store.GetState().users[carl.id].name);


            store.Dispatch(new ActionOnUser.ChangeAge() { targetUser = carlsFriend.id, newAge = null });
            Assert.Equal(3, usersChangedCounter);
            Assert.Null(store.GetState().users[carlsFriend.id].age);

            Assert.NotEqual(MyUser1.MyEnum.State2, store.GetState().users[carl.id].myEnum);
            store.Dispatch(new ActionOnUser.ChangeEnumState() { targetUser = carl.id, newEnumValue = MyUser1.MyEnum.State2 });
            Assert.Equal(MyUser1.MyEnum.State2, store.GetState().users[carl.id].myEnum);

            Log.MethodDone(t);
        }

        private class MyAppState1 {

            public readonly ImmutableDictionary<Guid, MyUser1> users;
            public readonly ImmutableArray<Guid?>? someUuids;

            public MyAppState1(ImmutableDictionary<Guid, MyUser1> users, ImmutableArray<Guid?>? someUuids) {
                this.users = users;
                this.someUuids = someUuids;
            }

        }

        private class MyUser1 {

            public enum MyEnum { State1, State2 }

            public readonly Guid id;
            public readonly string name;
            public readonly int? age;
            public readonly ImmutableList<Guid> contacts;
            public readonly MyEnum myEnum;

            public MyUser1(Guid id, string name, int? age, ImmutableList<Guid> contacts, MyEnum myEnum) {
                this.id = id;
                this.name = name;
                this.age = age;
                this.contacts = contacts;
                this.myEnum = myEnum;
            }

        }

        #region example actions

        private class ActionAddSomeId { public Guid? someId; }

        private class ActionOnUser {
            public Guid targetUser;

            public class ChangeName : ActionOnUser { public string newName; }
            public class ChangeAge : ActionOnUser { public int? newAge; }
            public class AddContact : ActionOnUser { public MyUser1 newContact; }
            public class ChangeEnumState : ActionOnUser { public MyUser1.MyEnum newEnumValue; }
        }

        #endregion // of example actions

        private static class MyReducers1 { // The reducers to modify the immutable datamodel:

            // The most outer reducer is public to be passed into the store:
            public static MyAppState1 ReduceMyAppState1(MyAppState1 myState, object action) {
                bool changed = false;
                var users = myState.users.Mutate(action, ReduceUsers, ref changed);
                var someIds = myState.someUuids.Mutate(action, ReduceSomeIds, ref changed);
                if (changed) { return new MyAppState1(users, someIds); }
                return myState;
            }

            private static ImmutableDictionary<Guid, MyUser1> ReduceUsers(ImmutableDictionary<Guid, MyUser1> users, object action) {
                if (action is ActionOnUser a) {
                    bool changed = false;
                    var user = users[a.targetUser].Mutate(action, ReduceUser, ref changed);
                    if (changed) {
                        users = users.SetItem(user.id, user);
                    } else {
                        throw new MissingMethodException("Logic for action missing: " + a);
                    }
                }
                if (action is ActionOnUser.AddContact add) {
                    // The AddContact action is handled in multiple reducers (here and below in the contacts reducer of the user):
                    users = users.AddOrCreate(add.newContact.id, add.newContact);
                }
                return users;
            }

            private static ImmutableArray<Guid?>? ReduceSomeIds(ImmutableArray<Guid?>? someIds, object action) {
                if (action is ActionAddSomeId a) {
                    if (someIds == null) { someIds = ImmutableArray<Guid?>.Empty; }
                    return someIds.Value.Add(a.someId);
                }
                return someIds;
            }

            private static MyUser1 ReduceUser(MyUser1 user, object action) {
                bool changed = false;
                var name = user.name.Mutate(action, ReduceUserName, ref changed);
                var age = user.age.Mutate(action, ReduceUserAge, ref changed);
                var contacts = user.contacts.Mutate(action, ReduceContacts, ref changed);
                var myEnum = user.myEnum.Mutate(action, ReduceMyEnum, ref changed);
                if (changed) {
                    // user.id can never change so always take it over from last state:
                    user = new MyUser1(user.id, name, age, contacts, myEnum);
                }
                return user;
            }

            private static string ReduceUserName(string oldName, object action) {
                if (action is ActionOnUser.ChangeName a) { return a.newName; }
                return oldName;
            }

            private static int? ReduceUserAge(int? oldAge, object action) {
                if (action is ActionOnUser.ChangeAge a) { return a.newAge; }
                return oldAge;
            }

            private static ImmutableList<Guid> ReduceContacts(ImmutableList<Guid> previousState, object action) {
                if (action is ActionOnUser.AddContact a) { return previousState.AddOrCreate(a.newContact.id); }
                return previousState;
            }

            private static MyUser1.MyEnum ReduceMyEnum(MyUser1.MyEnum previousState, object action) {
                if (action is ActionOnUser.ChangeEnumState a) { return a.newEnumValue; }
                return previousState;
            }

        }

    }

}