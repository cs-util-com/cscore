using com.csutil.model.immutable;
using com.csutil.progress;
using System;
using System.Collections.Immutable;
using System.Data;
using System.Threading.Tasks;
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

            store.Dispatch(new MyActions.SetCell("A", 3, "1"));
            store.Dispatch(new MyActions.SetCell("A", 2, "A3 + 1"));
            Assert.Throws<MyActions.SetCell.SelfRefException>(() => {
                store.Dispatch(new MyActions.SetCell("A", 3, "A2 + 1"));
            });
        }

        [Fact]
        public static void TestFromAndToRowName() {
            Assert.Equal(0, CellPos.ToColumnNr("A"));
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

        public static void SimulateSomeChangesInModel(DataStore<CellsModel> store) {
            store.Dispatch(new MyActions.SetCell("C", 3, "1 + 1"));
            store.Dispatch(new MyActions.SetCell("D", 4, "1 + C3"));
            store.Dispatch(new MyActions.SetCell("E", 5, "1 + D4"));
            store.Dispatch(new MyActions.SetCell("F", 6, "1 + E5"));
            store.Dispatch(new MyActions.SetCell("G", 8, "1 + F6"));
            store.Dispatch(new MyActions.SetCell("H", 9, "1 + G8"));
            // Then change the C3 chell which all other cells depend on:
            store.Dispatch(new MyActions.SetCell("C", 3, "2"));
        }

        public static async Task SimulateManyChangesInModel(DataStore<CellsModel> store, int nrOfChanges = 100) {
            var t = Log.MethodEnteredWith("nrOfChanges" + nrOfChanges);
            Log.e("SimulateManyChangesInModel");
            var random = new Random();
            var ops = new string[] { "+", "-", "*", "/" };
            var progress = ProgressUi.NewProgress(nrOfChanges);
            for (int i = 0; i < nrOfChanges; i++) {
                progress.IncrementCount();
                try {
                    int column = random.Next(1, 26 * 2);
                    int row = random.Next(1, 26 * 2);
                    string rndFormula = RndVar(random, store) + random.NextRndChild(ops) + RndVar(random, store);
                    store.Dispatch(new MyActions.SetCell(CellPos.ToColumnName(column), row, rndFormula));
                }
                catch (Exception e) { Log.e(e); }
                if (i % 5 == 0) { await TaskV2.Delay(1); } // Every 5 mutations wait to let the UI catch UI
            }
            Log.MethodDone(t);
        }

        /// <summary> Returns randomly a number or another cell reference </summary>
        private static string RndVar(System.Random random, DataStore<CellsModel> store) {
            if (random.NextBool()) { return "" + random.Next(-10, 10); }
            // With 50% prob. return a random other cell reference, currently in use:
            return "" + random.NextRndChild(store.GetState().cells).Key;
        }

    }

}