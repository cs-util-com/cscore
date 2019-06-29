using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace com.csutil.tests {

    public class TransducerTests {

        [Fact]
        public void Transducer_FilterMapReduce_Example() {

            Transducer<MyClass1, MyClass1> filter1 = Transducers.NewFilter<MyClass1>(x => x != null);
            Transducer<MyClass1, MyClass1> filter2 = Transducers.NewFilter<MyClass1>(x => x.someInt > 1);
            Transducer<MyClass1, int> mapper = Transducers.NewMapper<MyClass1, int>(x => x.someInt);
            Reducer<int> finalSumReducer = (total, x) => (int)total + x;

            // Create the reducer by composing the transducers:
            Reducer<MyClass1> createdReducer = filter1(filter2(mapper(finalSumReducer)));

            List<MyClass1> testData = newExampleList();
            var sum = createdReducer.Reduce(0, testData);
            Assert.Equal(6, sum);

        }

        [Fact]
        public void Transducer_FilterMap_Example() {

            var filter = Transducers.NewFilter<MyClass1>(x => x != null);
            var mapper = Transducers.NewMapper<MyClass1, int>(x => x.someInt);
            Reducer<MyClass1> createdReducer = filter(mapper.WithFinalListReducer());

            List<MyClass1> testData = newExampleList();
            var resultingList = createdReducer.Reduce(new List<int>(), testData);

            Assert.Equal(4, resultingList.Count());

        }

        [Fact]
        public void Transducer_Filter_Example() {



            var filter1 = Transducers.NewFilter<MyClass1>(x => x != null);
            var filter2 = Transducers.NewFilter<MyClass1>(x => x.someInt > 1);
            Reducer<MyClass1> createdReducer = filter1(filter2.WithFinalListReducer());

            List<MyClass1> testData = newExampleList();
            var resultingList = createdReducer.Reduce(new List<MyClass1>(), testData);

            Assert.Equal(3, resultingList.Count());

        }

        private class MyClass1 { public int someInt; }

        private static List<MyClass1> newExampleList() {
            var exampleList = new List<MyClass1>();
            exampleList.Add(new MyClass1() { someInt = 2 });
            exampleList.Add(null);
            exampleList.Add(new MyClass1() { someInt = 1 });
            exampleList.Add(new MyClass1() { someInt = 2 });
            exampleList.Add(new MyClass1() { someInt = 2 });
            return exampleList;
        }

    }

}