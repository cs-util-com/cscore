using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.model.immutable;
using Newtonsoft.Json.Linq;
using Xunit;
using Zio;

namespace com.csutil.tests.model {

    public class CloneTests {

        public CloneTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        private class MyClass1 : ICloneable {
            public string name;
            public int age;
            public MyClass1 child;
            public object Clone() { return this.MemberwiseClone(); }
        }

        [Fact]
        public void ExampleUsage1() {

            MyClass1 original = new MyClass1() { name = "1", child = new MyClass1() { name = "2", age = 3 } };

            MyClass1 copy = original.DeepCopyViaJson();
            Assert.Null(MergeJson.GetDiff(original, copy)); // No diff between original and copy
            AssertV2.AreEqualJson(original, copy); // AreEqualJson will use MergeJson.GetDiff internally
            Assert.Equal(original.child.name, copy.child.name);
            // Modify the copy, changing the copy will not change the original:
            copy.child.name = "Some new name..";
            // Check that the change was only done in the copy and not the original:
            Assert.NotEqual(original.child.name, copy.child.name);
            JToken diffToOriginal = MergeJson.GetDiff(original, copy);
            Assert.NotNull(diffToOriginal);

            // Objects that impl. IClonable can also ShallowCopy (will call .Clone internally):
            MyClass1 shallowCopy = original.ShallowCopyViaClone();
            Assert.NotSame(original, shallowCopy);
            Assert.Same(original.child, shallowCopy.child);

            // Applying a change to an existing target object is done using MergeJson.Patch:
            var oldName = original.child.name;
            MergeJson.Patch(original, diffToOriginal); // Apply the changes stored in the diff 
            Assert.NotEqual(oldName, original.child.name); // The name field was updated
            Assert.Equal(copy.child.name, original.child.name);
            Assert.Equal(3, original.child.age); // The age field was not changed by the patch

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
#pragma warning disable CS0612 // Type or member is obsolete
            CompareOriginalAndClone(dataTree, DeepCopyViaBsonStream(dataTree));
#pragma warning restore CS0612 // Type or member is obsolete

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

        [Obsolete]
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
            return new TreeElem { id = GuidV2.NewGuid().ToString(), name = nodeName, children = CreateChildren?.Invoke() };
        }

        [Fact]
        public void TestMakeDebugCopyOfAction() {
            {
                var actionToClone = new TestAction1("abc");
                bool copyOfActionSupported = false;
                object actionBeforeDispatch = null;
                Middlewares.MakeDebugCopyOfAction(actionToClone, ref copyOfActionSupported, ref actionBeforeDispatch);
                Assert.False(copyOfActionSupported);
            }
            {
                var actionToClone = new TestAction2("abc");
                bool copyOfActionSupported = false;
                object actionBeforeDispatch = null;
                Middlewares.MakeDebugCopyOfAction(actionToClone, ref copyOfActionSupported, ref actionBeforeDispatch);
                Assert.True(copyOfActionSupported);
            }
            {
                var objectToClone = new TestAction3() { SomeDir = EnvironmentV2.instance.GetNewInMemorySystem() };
                bool copyOfActionSupported = false;
                object actionBeforeDispatch = null;
                Middlewares.MakeDebugCopyOfAction(objectToClone, ref copyOfActionSupported, ref actionBeforeDispatch);
                Assert.False(copyOfActionSupported);
            }
        }

        public class TestAction1 {
            public string s { get; private set; }
            public TestAction1(string s1) { this.s = s1; }
        }

        public class TestAction2 {
            public string s { get; set; }
            public TestAction2(string s1) { this.s = s1; }
        }

        public class TestAction3 {
            public DirectoryEntry SomeDir { get; set; }
        }

    }

}