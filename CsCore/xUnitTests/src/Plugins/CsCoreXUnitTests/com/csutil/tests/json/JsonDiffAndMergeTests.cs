using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace com.csutil.tests.json {

    public class JsonDiffAndMergeTests {

        public JsonDiffAndMergeTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        class MyClass1 {
            public string myString;
            public string myString2 { get; set; }
            public MyClass1 complexField;
            public List<MyClass1> complexList;
        }

        [Fact]
        public void ExampleUsage1() {
            MyClass1 originalObj = new MyClass1() { myString = "abc", myString2 = "def" };

            MyClass1 copy1 = originalObj.DeepCopyViaJson();
            copy1.myString = "abcd";
            copy1.complexField = new MyClass1() { myString = "123", myString2 = "456" };
            copy1.complexField.complexList = new List<MyClass1>() { new MyClass1() { myString = "listEntry1" } };

            MyClass1 copy2 = originalObj.DeepCopyViaJson();
            copy2.myString2 = "defg";

            var merge = MergeJson.Merge(originalObj, copy1, copy2);
            Assert.False(merge.hasMergeConflict);

            // Parse the merged result back into a MyClass1 object:
            MyClass1 mergeResult1 = merge.GetResult();
            // The changes from both copies were merged correctly:
            Assert.Equal(copy1.myString, mergeResult1.myString);
            Assert.Equal(copy2.myString2, mergeResult1.myString2);
        }

        [Fact]
        public void ExampleUsage2() {
            MyClass1 originalObj = new MyClass1() { myString = "abc", myString2 = "def" };

            MyClass1 copy1 = originalObj.DeepCopyViaJson();
            copy1.myString = "abcd";
            copy1.complexField = new MyClass1() { myString = "123", myString2 = "456" };
            copy1.complexField.complexList = new List<MyClass1>() { new MyClass1() { myString = "listEntry1" } };

            MyClass1 copy2 = originalObj.DeepCopyViaJson();
            copy2.myString2 = "defg";
            copy2.myString = "123";
            copy2.complexField = new MyClass1() { myString = "zyx" };
            copy2.complexField.complexList = new List<MyClass1>() { new MyClass1() { myString = "listEntry2" } };

            var merge = MergeJson.Merge(originalObj, copy1, copy2);
            Assert.True(merge.hasMergeConflict);

            // Parsed conflicts returns an easy to iterate through array:
            var parsedConflicts = merge.GetParsedMergeConflicts();
            var firstConflict = parsedConflicts.First();
            Assert.Equal("myString", firstConflict.fieldName);
            Assert.Equal("abcd", "" + firstConflict.oldValue);
            Assert.Equal("123", "" + firstConflict.newValue);

            //Log.d("merge2.conflicts=" + JsonWriter.AsPrettyString(merge2.conflicts));
            //Log.d("parsedConflicts=" + JsonWriter.AsPrettyString(parsedConflicts));

        }

        [Fact]
        public void TestDiffV2CleanUp() {
            JToken jToken = JToken.Parse(@"{
                ""LocalPose"": {
                  ""M11"": [
                    -0.680480063,
                    -0.680480063
                  ],    
                  ""M43"": [
                    -5.162368,
                    -5.162368
                  ],
                  ""Translation"": {
                    ""Y"": [
                      1.819809,
                      1.819809
                    ],
                    ""Z"": [
                      -5.162368,
                      -5.162368
                    ]
                  }
                },
                ""Components"": {
                  ""39f7ac11-0bc1-4ac1-f8e9-08dc0b4858e8"": {
                    ""HeightInMeters"": [
                      1.9,
                      1.9
                    ]
                  }
                }
              }");
            JsonDiffPatchExtensions.CleanUp(jToken);
            Assert.True(jToken.IsNullOrEmpty(), "jToken=" + jToken);
        }

    }

}
