using System;
using System.Threading.Tasks;
using Xunit;
using com.csutil.random;
using System.Linq;

namespace com.csutil.tests.json {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class PersistedRegressionTests {

        public PersistedRegressionTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        class MyClass1 {
            public string myName;
            public MyClass1[] myChildren { get; set; }
        }

        [Fact]
        public async Task ExampleUsage1() {

            // First the test produces a state myState1:
            MyClass1 myState1 = new MyClass1() {
                myName = "abc",
                myChildren = new MyClass1[] {
                    new MyClass1() {  myName = "def" }
                }
            };
            // The regression test wants to ensure that this state does never change
            // Assertions ensure the correctness of the tested component
            Assert.NotNull(myState1.myName);

            // In addition the state can be persisted to detect any changes caused e.g. by future refactorings
            // To create such a persisted snapshot the PersistedRegression class can be used as follows:

            // The passed myState will be compared to the persisted regression entry stored on disk:
            await new PersistedRegression().AssertEqualToPersisted("PersistedRegressionTests_ExampleUsage1", myState1);
            // If the myState1 ever differs from how it was the first time the test was executed this will fail
            // This way changes have to be manually approved by the developer

            // The same regression test can also be triggered through the AssertV2 helper:
            AssertV2.IsEqualToPersisted("PersistedRegressionTests_ExampleUsage1", myState1);

        }

        [Fact]
        public async Task ExampleUsage2() {

            MyClass1 myState1 = new MyClass1() {
                myName = "fgh",
                myChildren = new MyClass1[] {
                    new MyClass1() { 
                        // Provoke a change in the state in each test execution:
                        myName = "" + new Random().NextRandomName()
                    }
                }
            };

            // Deleting old diff entries is possible like this:
            // await new PersistedRegression().regressionStore.Remove("PersistedRegressionTests_ExampleUsage2");

            var diffInStateWasAccepted = false;
            // A filter can be used to remove/filter out acceptable diffs from the report: 
            await new PersistedRegression().AssertEqualToPersisted("PersistedRegressionTests_ExampleUsage2", (diff) => {
                // Ignore differences in the field 'myChildren.0.myName' (has a random name in it):
                if (diff.Count() == 1 && diff.SelectToken("myChildren.0.myName") != null) {
                    diffInStateWasAccepted = true;
                    return false;
                }
                diffInStateWasAccepted = false;
                return true; // For all other diffs confirm that its a non acceptable diff
            }, myState1);
            Assert.True(diffInStateWasAccepted);

            // Make sure the regression entry was really created
            Assert.True(await new PersistedRegression().regressionStore.ContainsKey("PersistedRegressionTests_ExampleUsage2"));

        }

    }

}