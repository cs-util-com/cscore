using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace com.csutil.tests {

    public class TransducerTests {

        [Fact]
        public void Transducer_Filter_Example() {

            List<int> testData = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };

            var filter1 = Transducers.NewFilter<int>(x => x > 4);
            var filter2 = Transducers.NewFilter<int>(x => x % 2 != 0);
            {
                List<int> result = testData.FilterToList(Transducers.Compose(filter1, filter2));
                Assert.Equal(2, result.Count()); // 6 and 8 will be left
                Assert.Equal(5, result.First());
                Assert.Equal(7, result.Last());
            }
            { // without Transducers.Compose the filters have to be chained manually:
                List<int> result = testData.FilterToList(x => (filter1(filter2(x))));
                Assert.Equal(2, result.Count()); // 6 and 8 will be left
                Assert.Equal(5, result.First());
                Assert.Equal(7, result.Last());
            }
        }

        [Fact]
        public void Transducer_FilterMapReduce_Example() {

            List<int> testData = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };

            Transducer<int, int> filter1 = Transducers.NewFilter<int>(x => x > 4);
            Transducer<int, int> filter2 = Transducers.NewFilter<int>(x => x % 2 != 0);
            Transducer<int, float> mapper = Transducers.NewMapper<int, float>(x => x / 2f);
            Func<float, float, float> sumReducer = (total, x) => total + x;

            float result = testData.ReduceTo(x => filter1(filter2(mapper(x))), sumReducer, seed: 0);
            Assert.Equal(6, result); // 5/2 + 7/2 == 6

        }

        [Fact]
        public void Transducer_Filter_Example2() {

            List<MyClass1> testData = newExampleList();

            var filter1 = Transducers.NewFilter<MyClass1>(x => x != null);
            var filter2 = Transducers.NewFilter<MyClass1>(x => x.someInt > 1);

            List<MyClass1> resultingList = testData.FilterToList(x => filter1(filter2(x)));
            Assert.Equal(3, resultingList.Count());

        }

        [Fact]
        public void Transducer_FilterMap_Example() {

            List<MyClass1> testData = newExampleList();

            var filter = Transducers.NewFilter<MyClass1>(x => x != null);
            var mapper = Transducers.NewMapper<MyClass1, int>(x => x.someInt);

            List<int> resultingList = testData.MapToList<MyClass1, int>(x => filter(mapper(x)));
            Assert.Equal(4, resultingList.Count());

        }

        [Fact]
        public void Transducer_FilterMapReduce_Example2() {

            List<MyClass1> testData = newExampleList();

            Transducer<MyClass1, MyClass1> filter1 = Transducers.NewFilter<MyClass1>(x => x != null);
            Transducer<MyClass1, MyClass1> filter2 = Transducers.NewFilter<MyClass1>(x => x.someInt > 1);
            Transducer<MyClass1, int> mapper = Transducers.NewMapper<MyClass1, int>(x => x.someInt);
            Func<int, int, int> sumReducer = (total, x) => total + x;

            // Create the reducer by composing the transducers:
            var sum = testData.ReduceTo(x => filter1(filter2(mapper(x))), sumReducer, seed: 0);
            Assert.Equal(6, sum);

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