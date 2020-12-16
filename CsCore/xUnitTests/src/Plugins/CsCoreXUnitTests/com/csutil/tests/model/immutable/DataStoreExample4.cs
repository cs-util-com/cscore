using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    /// <summary> This example uses a mutable datamodel, which means that the 
    /// data store cant guarantee that the model wasn't modified outside of the reducers but allows to
    /// use the data store for models that partly have to be mutable (e.g. because they
    /// are not fully controlled by the developer). 
    /// 
    /// The main challange with mutable data is to propagate changes upwards through the model and mark
    /// all parent objects as changed as well. This example shows how this can be done following the 
    /// typical Reducer patterns also used very similarly with immutable data models. 
    /// </summary>
    public class DataStoreExample4 {

        public DataStoreExample4(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {
            var t = Log.MethodEntered("DataStoreExample3.ExampleUsage1");

            // A middleware that will allow to use mutable data in the data store:
            var thunkMiddleware = Middlewares.NewMutableDataSupport<MyAppState1>();

            // Add A logging middleware to log all dispatched actions:
            var loggingMiddleware = Middlewares.NewLoggingMiddleware<MyAppState1>();

            MyUser1 user = new MyUser1() { name = "Carl" };
            var model = new MyAppState1() { user = user };

            var store = new DataStore<MyAppState1>(MyReducers1.ReduceMyAppState1, model, loggingMiddleware, thunkMiddleware);
            IoC.inject.SetSingleton(store);
            store.storeName = "Store 4";

            // Setup 3 listeners that react when the name of the user or his contacts change:
            var userChangedCounter = 0;
            var userNameChangedCounter = 0;
            var contact1NameChangedCounter = 0;
            var contact2NameChangedCounter = 0;
            store.AddStateChangeListener(s => s.user, (MyUser1 theChangedUser) => {
                userChangedCounter++;
            });
            store.AddStateChangeListener(s => s.user?.name, (string theChangedName) => {
                userNameChangedCounter++;
            });
            store.AddStateChangeListener(s => s.user?.contacts?.FirstOrDefault()?.contactData.name, (_) => {
                contact1NameChangedCounter++;
            });
            store.AddStateChangeListener(s => s.user?.contacts?.Skip(1).FirstOrDefault()?.contactData.name, (_) => {
                contact2NameChangedCounter++;
            });

            var contact1 = new MyUser1() { name = "Tom" };
            { // Add a first contact to the user:
                Assert.Null(user.contacts);
                store.Dispatch(new ActionAddContact() {
                    targetUserId = user.id,
                    newContact = new MyContact1() { contactData = contact1 }
                });
                Assert.Same(contact1, user.contacts.First().contactData);
                Assert.True(user.WasModifiedInLastDispatch());

                // Now that there is a contact 1 the listener was triggered: 
                Assert.Equal(1, userChangedCounter);
                Assert.Equal(0, userNameChangedCounter);
                Assert.Equal(1, contact1NameChangedCounter);
                Assert.Equal(0, contact2NameChangedCounter);
            }

            var contact2 = new MyUser1() { name = "Bill" };
            { // Add a second contact to the user which should not affect contact 1:
                store.Dispatch(new ActionAddContact() {
                    targetUserId = user.id,
                    newContact = new MyContact1() { contactData = contact2 }
                });
                Assert.Same(contact2, user.contacts.Last().contactData);
                Assert.True(user.WasModifiedInLastDispatch());
                Assert.False(contact1.WasModifiedInLastDispatch());

                Assert.Equal(2, userChangedCounter);
                Assert.Equal(0, userNameChangedCounter);
                Assert.Equal(1, contact1NameChangedCounter);
                Assert.Equal(1, contact2NameChangedCounter);
            }
            { // Change the name of contact 1 which should not affect contact 2:
                var newName1 = "Toooom";
                store.Dispatch(new ActionChangeUserName() { targetUserId = contact1.id, newName = newName1 });
                Assert.True(user.WasModifiedInLastDispatch());
                Assert.True(contact1.WasModifiedInLastDispatch());
                Assert.False(contact2.WasModifiedInLastDispatch());

                Assert.Equal(3, userChangedCounter);
                Assert.Equal(0, userNameChangedCounter);
                Assert.Equal(2, contact1NameChangedCounter);
                Assert.Equal(1, contact2NameChangedCounter);
            }
            { // Change the name of the user which should not affect the 2 contacts:
                var newName = "Caaaarl";
                Assert.NotEqual(newName, user.name);
                Assert.Equal(user.name, store.GetState().user.name);
                var tBeforeDispatch = user.LastMutation;
                store.Dispatch(new ActionChangeUserName() { targetUserId = user.id, newName = newName });
                Assert.Equal(newName, store.GetState().user.name);
                Assert.Equal(newName, user.name);
                Assert.Same(model, store.GetState());

                Assert.NotEqual(tBeforeDispatch, user.LastMutation);
                Assert.True(user.WasModifiedInLastDispatch());
                Assert.False(contact1.WasModifiedInLastDispatch());
                Assert.False(contact2.WasModifiedInLastDispatch());

                Assert.Equal(4, userChangedCounter);
                Assert.Equal(1, userNameChangedCounter);
                Assert.Equal(2, contact1NameChangedCounter);
                Assert.Equal(1, contact2NameChangedCounter);
            }
            { // Marking an object mutated while not dispatching will throw an exception:
                Assert.Throws<InvalidOperationException>(() => {
                    user.name = "Cooorl";
                    user.MarkMutated();
                });
                Assert.Equal(4, userChangedCounter); // Count should not have changed
                Assert.Equal(1, userNameChangedCounter); 
            }
        }

        #region example Model (which is mutable)

        private class MyAppState1 : IsMutable {
            public MyUser1 user;
            public long LastMutation { get; set; }
        }

        private class MyUser1 : IsMutable {
            public Guid id { get; } = Guid.NewGuid();
            public string name;
            public List<MyContact1> contacts;
            public long LastMutation { get; set; }
        }

        private class MyContact1 : IsMutable {
            public MyUser1 contactData;
            public DateTime becameFriendsDate = DateTimeV2.Now;
            public long LastMutation { get; set; }
        }

        #endregion

        #region example actions

        private class ActionChangeUserName {
            public Guid targetUserId;
            public string newName;
        }

        private class ActionAddContact {
            public Guid targetUserId;
            public MyContact1 newContact;
        }

        #endregion // of example actions

        private static class MyReducers1 { // The reducers to modify the immutable datamodel:

            // The most outer reducer is public to be passed into the store:
            public static MyAppState1 ReduceMyAppState1(MyAppState1 model, object action) {
                bool changed = false;
                model.MutateField(model.user, action, (_, user, a) => ReduceUser(user, a), ref changed);
                if (changed) { model.MarkMutated(); }
                return model;
            }

            private static MyUser1 ReduceUser(MyUser1 user, object action) {
                var changed = false;
                if (action is ActionChangeUserName a) {
                    if (a.targetUserId == user.id) {
                        user.name = a.newName;
                        user.MarkMutated();
                        changed = true;
                    }
                }
                user.contacts = user.MutateField(user.contacts, action, ContactsReducer, ref changed);
                if (changed) { user.MarkMutated(); }
                return user;
            }

            private static List<MyContact1> ContactsReducer(MyUser1 parent, List<MyContact1> contacts, object action) {
                var changed = false;
                contacts.MutateEntries(action, ContactReducer, ref changed);
                if (action is ActionAddContact c) {
                    if (c.targetUserId == parent.id) {
                        if (contacts == null) { contacts = new List<MyContact1>(); }
                        contacts.Add(c.newContact);
                        changed = true;
                    }
                }
                if (changed) { parent.MarkMutated(); }
                return contacts;
            }

            private static MyContact1 ContactReducer(MyContact1 contact, object action) {
                var changed = false;
                contact.MutateField(contact.contactData, action, (_, user, a) => ReduceUser(user, a), ref changed);
                if (changed) { contact.MarkMutated(); }
                return contact;
            }

        }

    }

}