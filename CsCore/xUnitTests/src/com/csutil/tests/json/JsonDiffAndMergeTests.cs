using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using com.csutil.json;
using com.csutil.model;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace com.csutil.tests.json {

    public class JsonDiffAndMergeTests {

        public JsonDiffAndMergeTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        class MyClass1 {
            public string myString;
            public string myString2 { get; set; }
            public MyClass1 complexField;
        }

        [Fact]
        public void ExampleUsage1() {
            MyClass1 originalObj = new MyClass1() { myString = "abc", myString2 = "def" };

            var copy1 = originalObj.DeepCopyViaJson();
            copy1.myString = "abcd";
            copy1.complexField = new MyClass1() { myString = "123", myString2 = "456" };

            var copy2 = originalObj.DeepCopyViaJson();
            copy2.myString2 = "defg";

            var merge1 = MergeJson.Merge(originalObj, copy1, copy2);
            var mergeResult1 = merge1.result;
            // The changes from both copies were merged correctly:
            Assert.Equal(copy1.myString, mergeResult1.myString);
            Assert.Equal(copy2.myString2, mergeResult1.myString2);
            Assert.False(merge1.hasMergeConflict);

            var copy3 = copy2.DeepCopyViaJson();
            copy3.myString = "123";
            copy3.complexField = new MyClass1() { myString = "zyx" };

            var merge2 = MergeJson.Merge(originalObj, copy1, copy3);
            Assert.True(merge2.hasMergeConflict);

            var firstConflict = merge2.mergeConflicts.First();
            Assert.Equal("myString", firstConflict.fieldName);
            Assert.Equal("abcd", firstConflict.oldValue);
            Assert.Equal("123", firstConflict.newValue);

            Log.e("firstConflict=" + JsonWriter.AsPrettyString(firstConflict));
        }

    }

}
