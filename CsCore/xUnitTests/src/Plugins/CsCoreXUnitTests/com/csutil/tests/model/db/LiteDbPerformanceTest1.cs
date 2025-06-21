using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.model;
using UltraLiteDB;
using Xunit;
using Zio;

namespace com.csutil.integrationTests.model {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class LiteDbPerformanceTest1 {

        public LiteDbPerformanceTest1(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task PerformanceTest1() {
            var dataTree = NewTreeLayer("1", 1000, () => NewTreeLayer("2", 2, () => NewTreeLayer("3", 4, () => NewTreeLayer("4", 1))));

            var testFolder = EnvironmentV2.instance.GetOrAddTempFolder("tests.io.db").CreateV2();
            Log.d("Path=" + testFolder);
            var dbFile = testFolder.GetChild("PerformanceTestDB_" + GuidV2.NewGuid().ToString());

            // Open database (or create if doesn't exist)
            using (var db = new UltraLiteDatabase(dbFile.OpenOrCreateForReadWrite(), disposeStream: true)) {
                await InsertIntoDb(dataTree, db);
                await ReadFromDb(dataTree, db);
            }

            await WriteFiles(dataTree, testFolder);
            await ReadFiles(dataTree, testFolder);

            // cleanup after the test
            await ParallelExec(dataTree, (elem) => { GetFileForElem(testFolder, elem).DeleteV2(); });
            Assert.True(dbFile.DeleteV2());
            Assert.False(dbFile.Exists);
        }

        private static async Task ReadFromDb(List<TreeElem> dataTree, UltraLiteDatabase db) {
            var readTimer = Log.MethodEntered("LiteDbPerformanceTest1.ReadFromDb");
            var elements = db.GetCollection<TreeElem>("elements");
            await ParallelExec(dataTree, (elem) => {
                var found = elements.FindById(elem.id);
                Assert.Equal(elem.name, found.name);
            });
            Log.MethodDone(readTimer, 800);
        }

        private static async Task InsertIntoDb(List<TreeElem> dataTree, UltraLiteDatabase db) {
            var insertTimer = Log.MethodEntered("LiteDbPerformanceTest1.InsertIntoDb");
            var elements = db.GetCollection<TreeElem>("elements");
            object threadLock = new object();
            await ParallelExec(dataTree, (elem) => {
                lock (threadLock) {
                    elements.Insert(elem);
                }
            });
            Log.MethodDone(insertTimer, 4000);
            Assert.Equal(dataTree.Count, elements.FindAll().Count());
        }

        private static Task ParallelExec<T>(IEnumerable<T> data, Action<T> actionPerElement) {
            return Task.WhenAll(data.Map(elem => TaskV2.Run(() => actionPerElement(elem))));
        }

        private static async Task WriteFiles(List<TreeElem> dataTree, DirectoryEntry testFolder) {
            var insertTimer = Log.MethodEntered("LiteDbPerformanceTest1.WriteFiles");
            var jsonWriter = JsonWriter.GetWriter(null);
            await ParallelExec(dataTree, (elem) => {
                GetFileForElem(testFolder, elem).SaveAsJson(elem, jsonWriter);
            });
            Log.MethodDone(insertTimer, 7000);
        }

        private static async Task ReadFiles(List<TreeElem> dataTree, DirectoryEntry testFolder) {
            var readTimer = Log.MethodEntered("LiteDbPerformanceTest1.ReadFiles");
            var reader = JsonReader.GetReader(null);
            await ParallelExec(dataTree, (elem) => {
                var found = GetFileForElem(testFolder, elem).LoadAs<TreeElem>(reader);
                Assert.Equal(elem.name, found.name);
            });
            Log.MethodDone(readTimer, 4000);
        }

        private static FileEntry GetFileForElem(DirectoryEntry testFolder, TreeElem elem) {
            Assert.False(elem.id.IsNullOrEmpty());
            return testFolder.GetChild(elem.id + ".elem");
        }

        private class TreeElem : HasId {
            public string id { get; set; }
            public string name { get; set; }
            public List<TreeElem> children { get; set; }
            public string GetId() { return id; }
        }

        private List<TreeElem> NewTreeLayer(string layerName, int nodeCount, Func<List<TreeElem>> CreateChildren = null) {
            var l = new List<TreeElem>();
            for (int i = 1; i <= nodeCount; i++) { l.Add(NewTreeElem("Layer " + layerName + " - Node " + i, CreateChildren)); }
            return l;
        }

        private TreeElem NewTreeElem(string nodeName, Func<List<TreeElem>> CreateChildren = null) {
            return new TreeElem { id = GuidV2.NewGuid().ToString(), name = nodeName, children = CreateChildren?.Invoke() };
        }
    }

}