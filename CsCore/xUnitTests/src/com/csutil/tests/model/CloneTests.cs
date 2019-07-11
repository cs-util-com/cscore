using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.model;
using Xunit;

namespace com.csutil.tests.model {

    public class CloneTests {

        public CloneTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        private class MyClass1 {
            public string name;
            public MyClass1 child;
        }

        [Fact]
        public void ExampleUsage1() {

            MyClass1 original = new MyClass1() { name = "1", child = new MyClass1() { name = "2" } };

            MyClass1 copy = original.DeepCopyViaJsonString();
            Assert.Equal(original.child.name, copy.child.name);

            // Modify the original, changing the original will not change the copy
            original.child.name = "Some new name..";

            // Check that the change was only done in the original and not the copy:
            Assert.NotEqual(original.child.name, copy.child.name);

        }

        [Fact]
        public void PerformanceTest1() {

            var dataTree = NewTreeElem("root", () => NewTreeLayer("1", 20, () => NewTreeLayer("2", 200, () => NewTreeLayer("3", 2, () => NewTreeLayer("4", 10)))));

            CompareOriginalAndClone(dataTree, DeepCopySerializable(dataTree));
            CompareOriginalAndClone(dataTree, DeepCopyViaJson(dataTree));
            CompareOriginalAndClone(dataTree, DeepCopyViaJsonString(dataTree));
            CompareOriginalAndClone(dataTree, DeepCopyViaJsonOutString(dataTree));
            CompareOriginalAndClone(dataTree, DeepCopyViaBsonStream(dataTree));

        }

        private static void CompareOriginalAndClone(TreeElem original, TreeElem copy) {
            Assert.NotNull(copy.name);
            Assert.Equal(original.children.First().children.First().children.First().name, copy.children.First().children.First().children.First().name);
        }


        private static TreeElem DeepCopySerializable(TreeElem dataTree) {
            var t = Log.MethodEntered();
            var copy = dataTree.DeepCopySerializable();
            Log.MethodDone(t);
            return copy;
        }

        private static TreeElem DeepCopyViaJson(TreeElem dataTree) {
            var t = Log.MethodEntered();
            var copy = dataTree.DeepCopyViaJson();
            Log.MethodDone(t);
            return copy;
        }

        private static TreeElem DeepCopyViaJsonString(TreeElem dataTree) {
            var t = Log.MethodEntered();
            var copy = dataTree.DeepCopyViaJsonString();
            Log.MethodDone(t);
            return copy;
        }

        private static TreeElem DeepCopyViaJsonOutString(TreeElem dataTree) {
            var t = Log.MethodEntered();
            var copy = dataTree.DeepCopyViaJsonString(out string jsonString);
            Log.MethodDone(t);
            var testFile = EnvironmentV2.instance.GetOrAddTempFolder("DeepCopyViaJsonOutString").GetChild("test1.txt");
            testFile.SaveAsText(jsonString);
            Log.d("File " + testFile + " with size " + testFile.GetFileSizeString());
            return copy;
        }

        private static TreeElem DeepCopyViaBsonStream(TreeElem dataTree) {
            var t = Log.MethodEntered();
            var copy = dataTree.DeepCopyViaBsonStream();
            Log.MethodDone(t);
            return copy;
        }

        [Serializable]
        private class TreeElem {
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
            return new TreeElem { id = Guid.NewGuid().ToString(), name = nodeName, children = CreateChildren?.Invoke() };
        }

    }

}