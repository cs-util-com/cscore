using System.Collections.Generic;
using com.csutil.math;
using Xunit;

namespace com.csutil.tests {
    public class MathTests {

        public MathTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            List<float> someNumbers = new List<float>();
            float average = 0;

            average = AddValue(someNumbers, 4, average);
            Assert.Equal(4f, average);
            average = AddValue(someNumbers, 6, average);
            Assert.Equal(5f, average);
            average = AddValue(someNumbers, 8, average);
            Assert.Equal(6f, average);

        }

        private static float AddValue(List<float> self, float newValue, float oldAverage) {
            var newAverage = CalculateAverage.CalcRunningMean(oldAverage, newValue, self.Count);
            self.Add(newValue);
            return newAverage;
        }

        [Fact]
        public void TestMathEvaluator() {
            Assert.Equal(9, Numbers.Calculate("1 + 2 * 4"));
            Assert.Equal(1, Numbers.Calculate("(1 + 2 * 4) % 2"));
            Assert.Equal(4.5, Numbers.Calculate("(1 + 2 * 4) / 2"));
            Assert.Equal(1, Numbers.Calculate("0.5 + 0.5"));
            Assert.Equal(1, Numbers.Calculate("0.5 + 0,5"));
            Assert.Equal(1, Numbers.Calculate("0,5 + 0,5"));
            Assert.Equal(0.6667, Numbers.Calculate("2/3"), 4);
            Assert.NotEqual(0.667, Numbers.Calculate("2/3"), 4);
        }

        [Fact]
        public static void TestDataTable() {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("A", typeof(int));
            dt.Columns.Add("B", typeof(int));
            dt.Rows.Add(11, 12); // Insert a row with A=4, B=1
            // Querying the table for specific entries:
            var boolResult = dt.Select("A>B-2").Length > 0;
            Assert.True(boolResult); // 11 > 12-2
            // Add a result column that calculates a formula based on the entries:
            var columnName = "Result Column";
            dt.Columns.Add(columnName, typeof(int), "A+B*2");
            var rowNr = 0;
            var valResult = dt.Rows[rowNr][columnName];
            Assert.Equal(35, valResult); // 11 + 12*2  = 35
        }

    }

}