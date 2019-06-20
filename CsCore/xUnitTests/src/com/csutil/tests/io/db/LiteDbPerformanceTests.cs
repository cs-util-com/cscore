using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.io.db;
using LiteDB;
using Xunit;

namespace com.csutil.tests.io.db {

    public class LiteDbPerformanceTests {

        private class Elem : HasId {
            public string id { get; set; }
            public string name { get; internal set; }
            public List<Elem> children { get; set; }
            public string GetId() { return id; }
        }

        [Fact]
        async void PerformanceTest1() {
            AssertV2.throwExeptionIfAssertionFails = true;
            
            var dataTree = NewTreeLayer("1", 1000, () => NewTreeLayer("2", 2, () => NewTreeLayer("3", 4, () => NewTreeLayer("4", 1))));

            var dbFile = EnvironmentV2.instance.GetAppDataFolder().GetChildDir("tests.io.db").GetChild("PerformanceTestDB_" + Guid.NewGuid().ToString());
            dbFile.ParentDir().CreateV2();

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(dbFile.FullPath())) {
                await insertIntoDb(dataTree, db);
                await readFromDb(dataTree, db);
            }
            Assert.True(dbFile.IsNotNullAndExists());
            dbFile.DeleteV2(); // cleanup after the test
        }

        private static async Task readFromDb(List<Elem> dataTree, LiteDatabase db) {
            var readTimer = Log.MethodEntered("Insert into DB");
            var elements = db.GetCollection<Elem>("elements");
            var readTasks = dataTree.Map(x => Task.Run(() => {
                var found = elements.FindById(x.id);
                Assert.Equal(x.name, found.name);
            }));
            await Task.WhenAll(readTasks);
            Log.MethodDone(readTimer, 200);
        }

        private static async Task insertIntoDb(List<Elem> dataTree, LiteDatabase db) {
            var insertTimer = Log.MethodEntered("Insert into DB");
            var elements = db.GetCollection<Elem>("elements");
            var insertTasks = dataTree.Map(elem => Task.Run(() => {
                elements.Insert(elem);
            }));
            await Task.WhenAll(insertTasks);
            Log.MethodDone(insertTimer, 500);
        }

        private List<Elem> NewTreeLayer(string layerName, int nodeCount, Func<List<Elem>> CreateChildren = null) {
            var l = new List<Elem>();
            for (int i = 1; i <= nodeCount; i++) { l.Add(NewTreeElem("Layer " + layerName + " - Node " + i, CreateChildren)); }
            return l;
        }

        private Elem NewTreeElem(string nodeName, Func<List<Elem>> CreateChildren = null) {
            return new Elem { id = Guid.NewGuid().ToString(), name = nodeName, children = CreateChildren?.Invoke() };
        }
    }

}