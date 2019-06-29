using System.Collections.Generic;
using Xunit;

namespace com.csutil.tests {

    public class TransducerTests {

        private class MyClass1 {
            public int someInt;
        }

        [Fact]
        public void Transducer_Examples() {

            var filter1 = Transducers.NewFilter<MyClass1, int>(x => x != null);
            var filter2 = Transducers.NewFilter<MyClass1, int>(x => x.someInt > 1);
            var mapper = Transducers.NewMapper<MyClass1, int>(x => x.someInt);
            Transducers.Reducer<int, int> reduceToTotalSum = (total, x) => total + x;

            // Create the reducer by composing the transducers:
            var allInOneReducer = filter1(filter2(mapper(reduceToTotalSum)));

            var testData = new List<MyClass1>();
            testData.Add(new MyClass1() { someInt = 2 });
            testData.Add(null);
            testData.Add(new MyClass1() { someInt = 1 });
            testData.Add(new MyClass1() { someInt = 2 });
            testData.Add(new MyClass1() { someInt = 2 });

            var sum = allInOneReducer.Reduce(testData, 0);
            Assert.Equal(6, sum);

        }

    }

}