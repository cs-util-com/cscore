using System;
using System.Collections.Immutable;
using System.Linq;
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

            var data = new MyAppState1(new MyUser("Bob", null));
            var store = new DataStore<MyAppState1>(MyReducers1.ReduceMyAppState1, data);

            var user = store.GetSubState(data => data.User);
            Assert.NotNull(user.State);

            int userNameCounter = 0;
            user.AddStateChangeListener(user => user.Name, userName => {
                userNameCounter++;
                // First time name will be Bob and second time Alice:
                if (userNameCounter == 1) { Assert.Equal("Bob", userName); }
                if (userNameCounter == 2) { Assert.Equal("Alice", userName); }
            }, triggerInstantToInit: true);

            // The user has a dog that can be observed via another substate object:
            var dog = user.GetSubState(user => user.Dog);

            // Since the dog substate object was created as a child of the user substate object,
            // its onStateChanged listener will only be triggered if the user substate object changes:
            int dogChangedCounter = 0;
            dog.onStateChanged += (newDog) => { dogChangedCounter++; };
            dog.AddStateChangeListener(dog => dog?.Name, dogName => {
                // First the user has no dog, then a dog named Max, then a dog named Rex:
                if (dogChangedCounter == 1) { Assert.Equal("Max", dogName); }
                if (dogChangedCounter == 2) { Assert.Equal("Rex", dogName); }
            }, triggerInstantToInit: false);

            Assert.Equal("Bob", user.State.Name);
            Assert.Equal(1, userNameCounter);
            user.Dispatch(new ActionChangeName("Alice"));
            Assert.Equal("Alice", user.State.Name);
            Assert.Equal(2, userNameCounter);

            Assert.Null(dog.State);
            Assert.Equal(0, dogChangedCounter);
            user.Dispatch(new ActionAdoptDog(new Dog("Max")));
            Assert.Equal("Max", dog.State.Name);
            Assert.Equal(1, dogChangedCounter);
            user.Dispatch(new ActionAdoptDog(new Dog("Rex")));
            Assert.Equal("Rex", dog.State.Name);
            Assert.Equal(2, dogChangedCounter);

        }

        private class MyAppState1 {
            public readonly MyUser User;
            public MyAppState1(MyUser user) { User = user; }
        }

        private class MyUser {
            public readonly string Name;
            public readonly Dog Dog;
            public MyUser(string name, Dog dog) {
                Name = name;
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

        private class MyReducers1 {

            public static MyAppState1 ReduceMyAppState1(MyAppState1 previousstate, object action) {
                if (action is ActionChangeName a) {
                    return new MyAppState1(new MyUser(a.NewName, previousstate.User.Dog));
                }
                if (action is ActionAdoptDog b) {
                    return new MyAppState1(new MyUser(previousstate.User.Name, b.NewPet));
                }
                return previousstate;
            }

        }

    }

}