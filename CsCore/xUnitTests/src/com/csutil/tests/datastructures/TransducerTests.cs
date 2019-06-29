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

            var filter1 = Transducers.NewFilter<MyClass1, int>(x => x != null);
            var filter2 = Transducers.NewFilter<MyClass1, int>(x => x.someInt > 1);
            var mapper = Transducers.NewMapper<MyClass1, int, int>(x => x.someInt);
            // Create the reducer by composing the transducers:
            Reducer<MyClass1, int> allInOneReducer = filter1(filter2(mapper((total, x) => total + x)));

            List<MyClass1> testData = newExampleList();
            var sum = allInOneReducer.Reduce(0, testData);
            Assert.Equal(6, sum);

        }

        [Fact]
        public void Transducer_Example2() {

            var filter = Transducers.NewFilter<MyClass1, List<int>>(x => x != null);
            var mapper = Transducers.NewMapper<MyClass1, int, List<int>>(x => x.someInt);
            Reducer<MyClass1, List<int>> allInOneReducer = filter(mapper.NewMapperToList());

            List<MyClass1> testData = newExampleList();
            var resultingList = allInOneReducer.Reduce(new List<int>(), testData);

            Assert.Equal(4, resultingList.Count());

        }

    }

}