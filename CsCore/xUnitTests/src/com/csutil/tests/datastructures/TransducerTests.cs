using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace com.csutil.tests {

    public class TransducerTests {

        private class MyClass1 {
            public int someInt;
        }

        private static List<MyClass1> newExampleList() {
            var testData = new List<MyClass1>();
            testData.Add(new MyClass1() { someInt = 2 });
            testData.Add(null);
            testData.Add(new MyClass1() { someInt = 1 });
            testData.Add(new MyClass1() { someInt = 2 });
            testData.Add(new MyClass1() { someInt = 2 });
            return testData;
        }

        [Fact]
        public void Transducer_Example1() {

            var filter1 = Transducers.NewFilter<MyClass1>(x => x != null);
            var filter2 = Transducers.NewFilter<MyClass1>(x => x.someInt > 1);
            var mapper = Transducers.NewMapper<MyClass1, int>(x => x.someInt);
            Reducer<int> finalSumReducer = (total, x) => (int)total + x;
            // Create the reducer by composing the transducers:
            Reducer<MyClass1> allInOneReducer = filter1(filter2(mapper(finalSumReducer)));

            List<MyClass1> testData = newExampleList();
            var sum = allInOneReducer.Reduce(0, testData);
            Assert.Equal(6, sum);

        }

        [Fact]
        public void Transducer_Example2() {

            var filter = Transducers.NewFilter<MyClass1>(x => x != null);
            var mapper = Transducers.NewMapper<MyClass1, int>(x => x.someInt);
            Reducer<MyClass1> allInOneReducer = filter(mapper.WithFinalListReducer());

            List<MyClass1> testData = newExampleList();
            var resultingList = allInOneReducer.Reduce(new List<int>(), testData);

            Assert.Equal(4, resultingList.Count());

        }

        [Fact]
        public void Transducer_Example3() {

            var filter1 = Transducers.NewFilter<MyClass1>(x => x != null);
            var filter2 = Transducers.NewFilter<MyClass1>(x => x.someInt > 1);
            Reducer<MyClass1> allInOneReducer = filter1(filter2.WithFinalListReducer());

            List<MyClass1> testData = newExampleList();
            var resultingList = allInOneReducer.Reduce(new List<MyClass1>(), testData);

            Assert.Equal(3, resultingList.Count());

        }

    }

}