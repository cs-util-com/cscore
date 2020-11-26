using com.csutil.model.immutable;
using System.Collections.Immutable;
using System.Data;
using Xunit;

namespace com.csutil.tests.Task7 {
    public class CellsModelTests {

        [Fact]
        public static void TestDataStoreTransitiveChanges() {

            CellsModel model = new CellsModel(ImmutableDictionary<CellPos, Cell>.Empty);
            var store = new DataStore<CellsModel>(MyReducers.MyReducer, model, Middlewares.NewLoggingMiddleware<CellsModel>());

            store.Dispatch(new MyActions.SetCell("A", 1, "1 + 1"));
            var cells = store.SelectElement(s => s.cells);
            Cell a1 = cells()[new CellPos("A", 1)];
            Assert.Equal(2, a1.value);

            // B1 will depend on A1:
            store.Dispatch(new MyActions.SetCell("B", 1, "3 * A1"));
            Cell b1 = cells()[new CellPos("B", 1)];
            Assert.Equal(6, b1.value);

            // C1 will depend on A1 and B1:
            store.Dispatch(new MyActions.SetCell("C", 1, "B1 + 3 - A1"));
            Cell c1 = cells()[new CellPos("C", 1)];
            Assert.Equal(7, c1.value);

            // D1 will depend on C1 (so transitivly also on A1 and B1):
            store.Dispatch(new MyActions.SetCell("D", 1, "2 * C1"));
            Cell d1 = cells()[new CellPos("D", 1)];
            Assert.Equal(14, d1.value);

            // Now changing A1 must have affects to B1, C1, D1 as well:
            store.Dispatch(new MyActions.SetCell("A", 1, "4 + 1"));
            c1 = cells()[new CellPos("C", 1)];
            Assert.Equal(13, c1.value);
            d1 = cells()[new CellPos("D", 1)];
            Assert.Equal(26, d1.value);

            // Select cell C1:
            store.Dispatch(new MyActions.SelectCell(c1.pos));
            Assert.True(cells()[new CellPos("C", 1)].isSelected);
            // Select cell D1:
            store.Dispatch(new MyActions.SelectCell(d1.pos));
            Assert.True(cells()[new CellPos("D", 1)].isSelected);
            Assert.False(cells()[new CellPos("C", 1)].isSelected);

        }

        [Fact]
        public static void TestFromAndToRowName() {
            Assert.Equal("A", CellPos.ToColumnName(CellPos.ToColumnNr("A")));
            Assert.Equal("A", CellPos.ToColumnName(CellPos.ToColumnNr("a")));
            Assert.Equal("Z", CellPos.ToColumnName(CellPos.ToColumnNr("Z")));
            Assert.Equal("AA", CellPos.ToColumnName(CellPos.ToColumnNr("AA")));
            Assert.Equal("AB", CellPos.ToColumnName(CellPos.ToColumnNr("AB")));
            Assert.Equal("AZ", CellPos.ToColumnName(CellPos.ToColumnNr("AZ")));
        }

        /// <summary> Data table would be an alternative to implement the complete Task 7, to 
        /// test & demonstrate the usability of Redux the DataTable class was only used for the 
        /// final formula calculations like 1+1 </summary>
        [Fact]
        public static void TestDataTable() {
            Assert.Equal(9, Numbers.Calculate("1 + 2 * 4"));

            var dt = new DataTable();
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