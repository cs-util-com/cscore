using Xunit;

namespace com.csutil.tests {

    public class PointersExample {

        public PointersExample(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public unsafe void Example1MarkedAsUnsafeCode() {
            int myVariable1 = 123;
            int myVariable2 = 456;

            int* pointerToMyVariable = &myVariable1;
            int valueAtPointer = *pointerToMyVariable;
            Assert.Equal(myVariable1, valueAtPointer);

            // Pointing the pointer to a different place in memory:
            pointerToMyVariable = &myVariable2;
            valueAtPointer = *pointerToMyVariable;
            Assert.Equal(myVariable2, valueAtPointer);

            // Calling methods on the object the pointer points to:
            string intAsString = pointerToMyVariable->ToString();
            Assert.Equal("" + myVariable2, intAsString);
        }

    }

}