using System;
using System.Collections.Immutable;
using System.Linq;
using com.csutil.model.immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    public class SubStateTests {

        public SubStateTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            var data = new MyAppState1(new MyUser("Bob"));
            var store = new DataStore<MyAppState1>(MyReducers1.ReduceMyAppState1, data);

            var user = store.GetSubState(data => data.User);
            Assert.NotNull(user.State);

            int counter = 0;
            user.AddStateChangeListener(user => user.Name, name => {
                counter++;
                // First time name will be Bob and second time Alice:
                if (counter == 1) { Assert.Equal("Bob", name); }
                if (counter == 2) { Assert.Equal("Alice", name); }
            }, triggerInstantToInit: true);
            Assert.Equal(1, counter);
            user.Dispatch(new ActionChangeName("Alice"));
            Assert.Equal(2, counter);

        }

        private class MyAppState1 {
            public readonly MyUser User;
            public MyAppState1(MyUser user) { User = user; }
        }

        private class MyUser {
            public readonly string Name;
            public MyUser(string name) { Name = name; }
        }

        public class ActionChangeName {
            public readonly string newName;
            public ActionChangeName(string newName) { this.newName = newName; }
        }

        private class MyReducers1 {

            public static MyAppState1 ReduceMyAppState1(MyAppState1 previousstate, object action) {
                if (action is ActionChangeName a) {
                    return new MyAppState1(new MyUser(a.newName));
                }
                return previousstate;
            }

        }

    }

}