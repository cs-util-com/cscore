using System;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    /// <summary>
    /// These tests show the usage of the SubState class, which allows a syntax for immutable objects that
    /// is more similar to mutable objects. Actions can be dispatched directly on the SubState object and
    /// changes to the SubState object can be observed via a listener or by directly accessing the
    /// State property after the action was dispatched.
    /// </summary>
    public class SubStateTests {

        public SubStateTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            // Create some example app state and put it into a store:
            var state = new MyAppState1(user: new MyUser(userName: "Bob", dog: null), someNumber: 123);
            DataStore<MyAppState1> store = new DataStore<MyAppState1>(MyReducers1.ReduceMyAppState1, state);

            // Accessing sub states of the entire state is done via the GetSubState method:
            SubState<MyAppState1, MyUser> userState = store.GetSubState(data => data.User);
            
            // The most relevant property of the SubState is the GetState Func which
            // will always return the latest state from the store if used like this:
            Func<MyUser> user = userState.GetState;
            Assert.Equal("Bob", user().UserName);
            // Using the Func like this (user().UserName) is a little bit more readable compared to
            // going through userState every time but results in the same:
            Assert.Equal("Bob", userState.GetState().UserName);

            // Every time the user changes in the store the onStateChanged listener will be triggered:
            int userChangedCounter = 0;
            userState.onStateChanged += () => { userChangedCounter++; };

            // The same way as listening to the store, the user substate can be listened to:
            int userNameCounter = 0;
            userState.AddStateChangeListener(u => u.UserName, userName => {
                userNameCounter++;
                // First time name will be Bob and second time Alice:
                if (userNameCounter == 1) { Assert.Equal("Bob", userName); }
                if (userNameCounter == 2) { Assert.Equal("Alice", userName); }
            }, triggerInstantToInit: true);

            // The user has a dog that can be observed via another substate object:
            SubState<MyAppState1, Dog> dogState = userState.GetSubState(u => u.Dog);
            Func<Dog> dog = dogState.GetState;
            
            // Since the dog substate object was created as a child of the user substate object,
            // its onStateChanged listener will only be triggered if the user substate object changes:
            int dogChangedCounter = 0;
            dogState.onStateChanged += () => { dogChangedCounter++; };
            dogState.AddStateChangeListener(d => d?.Name, dogName => {
                // First the user has no dog, then a dog named Max, then a dog named Rex:
                if (dogChangedCounter == 1) { Assert.Equal("Max", dogName); }
                if (dogChangedCounter == 2) { Assert.Equal("Rex", dogName); }
            }, triggerInstantToInit: false);

            Assert.Equal(0, userChangedCounter);
            // Because triggerInstantToInit was set to true the name counter will already be triggered once:
            Assert.Equal(1, userNameCounter);

            Assert.Equal("Bob", user().UserName);
            userState.Dispatch(new ActionChangeName("Alice"));
            Assert.Equal("Alice", user().UserName);
            
            Assert.Equal(2, userNameCounter);
            Assert.Equal(1, userChangedCounter);

            Assert.Equal(0, dogChangedCounter);

            Assert.Null(dog());
            userState.Dispatch(new ActionAdoptDog(new Dog("Max")));
            Assert.Equal("Max", dog().Name);
            Assert.Equal(1, dogChangedCounter);
            Assert.Equal(2, userChangedCounter);

            userState.Dispatch(new ActionAdoptDog(new Dog("Rex")));
            Assert.Equal("Rex", dog().Name);
            Assert.Equal(2, dogChangedCounter);
            Assert.Equal(3, userChangedCounter);

            // Changing other parts of the store (like the SomeNumber) does not trigger any of the SubState listeners:
            Assert.Equal(3, userChangedCounter);
            Assert.Equal(123, store.GetState().SomeNumber);

            store.Dispatch(new ActionChangeNumber(456));
            Assert.Equal(456, store.GetState().SomeNumber);
            Assert.Equal(3, userChangedCounter);

            // Over all the mutations that happened on the user and dog the state object and functions did not change:
            Assert.Same(user, userState.GetState);
            Assert.Same(dog, dogState.GetState);
            
            // Disposing the user SubState will make it (and all its SubSubStates) unusable:
            userState.Dispose();
            Assert.Equal(DisposeState.Disposed, userState.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => { userState.Dispatch(new ActionChangeName("Alice")); });
            Assert.Throws<ObjectDisposedException>(() => { user(); });
            Assert.Throws<ObjectDisposedException>(() => { dog(); });
            store.Dispatch(new ActionAdoptDog(new Dog("Max")));
            store.Dispatch(new ActionChangeName("Bob"));
            // The userChangedCounter will not be triggered because the user was disposed:
            Assert.Equal(3, userChangedCounter);
            Assert.Equal(2, userNameCounter);
            // The dogChangedCounter will also not be triggered because the user was disposed:
            Assert.Equal(2, dogChangedCounter);

            // The dog substate is already unusable but still can be disposed
            dogState.Dispose();
            Assert.Equal(DisposeState.Disposed, dogState.IsDisposed);

        }

        [Fact]
        public void TestSubStateSubscription() {

            // Create some example app state and put it into a store:
            var state = new MyAppState1(user: new MyUser(userName: "Bob", dog: null), someNumber: 123);
            DataStore<MyAppState1> store = new DataStore<MyAppState1>(MyReducers1.ReduceMyAppState1, state);

            // Accessing sub states of the entire state is done via the GetSubState method:
            SubState<MyAppState1, MyUser> userState = store.GetSubState(data => data.User);

            // Creating the substate does not instantly subscribe it to the store:
            Assert.Null(userState.RemoveFromParent);
            Assert.Null(store.onStateChanged);

            // Only when either the substate or a new child subsubstate is created, subscribing the substate to the store is needed:
            SubState<MyAppState1, Dog> dogState = userState.GetSubState(u => u.Dog);
            Assert.NotNull(userState.RemoveFromParent);
            var actionAsMultpleEntities = store.onStateChanged.GetInvocationList();
            Assert.Single(actionAsMultpleEntities);

            // Adding another subsubsubstate will not add another listener to the store:
            SubState<MyAppState1, string> dogNameState = dogState.GetSubState(d => d?.Name);
            Assert.Single(store.onStateChanged.GetInvocationList());

            SubState<MyAppState1, int> someNumberState = store.GetSubState(data => data.SomeNumber);
            // Just using the SubState object as a way to access always the latest state in the store does not register it yet as a listener:
            Assert.Equal(123, someNumberState.GetState());
            Assert.Single(store.onStateChanged.GetInvocationList());
            // Registering a callback on the SubState will register the SubState on the store
            someNumberState.onStateChanged += () => { };
            Assert.Equal(2, store.onStateChanged.GetInvocationList().Length);

        }

        private class MyAppState1 {
            public readonly MyUser User;
            public readonly int SomeNumber;
            public MyAppState1(MyUser user, int someNumber) {
                User = user;
                SomeNumber = someNumber;
            }
        }

        private class MyUser {
            public readonly string UserName;
            public readonly Dog Dog;
            public MyUser(string userName, Dog dog) {
                UserName = userName;
                Dog = dog;
            }
        }

        private class Dog {
            public readonly string Name;
            public Dog(string name) { Name = name; }
        }

        private class ActionChangeName {
            public readonly string NewName;
            public ActionChangeName(string newName) { NewName = newName; }
        }

        private class ActionAdoptDog {
            public readonly Dog NewPet;
            public ActionAdoptDog(Dog newPet) { NewPet = newPet; }
        }

        private class ActionChangeNumber {
            public readonly int NewNumber;
            public ActionChangeNumber(int newNumber) { NewNumber = newNumber; }
        }

        private class MyReducers1 {

            public static MyAppState1 ReduceMyAppState1(MyAppState1 previousstate, object action) {
                if (action is ActionChangeName a) {
                    return new MyAppState1(new MyUser(a.NewName, previousstate.User.Dog), previousstate.SomeNumber);
                }
                if (action is ActionAdoptDog b) {
                    return new MyAppState1(new MyUser(previousstate.User.UserName, b.NewPet), previousstate.SomeNumber);
                }
                if (action is ActionChangeNumber c) {
                    return new MyAppState1(previousstate.User, c.NewNumber);
                }
                return previousstate;
            }

        }

    }

}