using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.model;
using Xunit;

namespace com.csutil.tests.model {

    public class CloneTests {

        public CloneTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        private class MyClass1 : ICloneable {
            public string name;
            public MyClass1 child;
            public object Clone() { return this.MemberwiseClone(); }
        }

        [Fact]
        public void ExampleUsage1() {

            MyClass1 original = new MyClass1() { name = "1", child = new MyClass1() { name = "2" } };

            MyClass1 copy = original.DeepCopyViaJson();
            Assert.Equal(original.child.name, copy.child.name);
            // Modify the copy, changing the copy will not change the original:
            copy.child.name = "Some new name..";
            // Check that the change was only done in the copy and not the original:
            Assert.NotEqual(original.child.name, copy.child.name);

            // Objects that impl. IClonable can also ShallowCopy (will call .Clone internally):
            MyClass1 shallowCopy = original.ShallowCopyViaClone();
            Assert.NotSame(original, shallowCopy);
            Assert.Same(original.child, shallowCopy.child);

        }

        private class MyClass2 {
            public string name;
            public MyClass2 child;
        }

        [Fact]
        public void ExampleUsage2() {

            MyClass2 originalClass2 = new MyClass2() { name = "1", child = new MyClass2() { name = "2" } };

            MyClass1 mappedClass1 = originalClass2.MapViaJsonInto<MyClass1>();
            Assert.Equal(originalClass2.child.name, mappedClass1.child.name);
            // Modify the copy, changing the copy will not change the original:
            mappedClass1.child.name = "Some new name..";
            // Check that the change was only done in the copy and not the original:
            Assert.NotEqual(originalClass2.child.name, mappedClass1.child.name);

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
            var t = Log.MethodEntered("DeepCopySerializable");
            var copy = CloneHelper.DeepCopySerializable(dataTree);
            Log.MethodDone(t);
            return copy;
        }

        private static TreeElem DeepCopyViaJson(TreeElem dataTree) {
            var t = Log.MethodEntered("DeepCopyViaJson");
            var copy = dataTree.DeepCopyViaJson();
            Log.MethodDone(t);
            return copy;
        }

        private static TreeElem DeepCopyViaJsonString(TreeElem dataTree) {
            var t = Log.MethodEntered("DeepCopyViaJsonString");
            var copy = CloneHelper.DeepCopyViaJsonString(dataTree);
            Log.MethodDone(t);
            return copy;
        }

        private static TreeElem DeepCopyViaJsonOutString(TreeElem dataTree) {
            var t = Log.MethodEntered("DeepCopyViaJsonOutString");
            var copy = CloneHelper.DeepCopyViaJsonString(dataTree, out string jsonString);
            Log.MethodDone(t);
            var testFile = EnvironmentV2.instance.GetOrAddTempFolder("DeepCopyViaJsonOutString").GetChild("test1.txt");
            testFile.SaveAsText(jsonString);
            Log.d("File " + testFile + " with size " + testFile.GetFileSizeString());
            return copy;
        }

        private static TreeElem DeepCopyViaBsonStream(TreeElem dataTree) {
            var t = Log.MethodEntered("DeepCopyViaBsonStream");
            var copy = CloneHelper.DeepCopyViaBsonStream(dataTree);
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