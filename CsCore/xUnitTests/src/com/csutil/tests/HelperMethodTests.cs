using System;
using System.Collections.Generic;
using Xunit;

namespace com.csutil.tests {
    public class HelperMethodTests : IDisposable {

        public HelperMethodTests() { // // Setup before each test:
        }

        public void Dispose() { // TearDown after each test:
        }

        [Fact]
        public void DelegateExtensionTests() {
            {
                Action a = null;
                Assert.False(a.InvokeIfNotNull());
                a = () => { };
                Assert.True(a.InvokeIfNotNull());
            }
            {
                Action<string> a = null;
                Assert.False(a.InvokeIfNotNull(""));
                a = (s) => { };
                Assert.True(a.InvokeIfNotNull(""));
            }
            {
                Action<string, int> a = null;
                Assert.False(a.InvokeIfNotNull("", 123));
                a = (s, i) => { };
                Assert.True(a.InvokeIfNotNull("", 123));
            }
            {
                Action<string, int, bool> a = null;
                Assert.False(a.InvokeIfNotNull("", 123, true));
                a = (s, i, b) => { };
                Assert.True(a.InvokeIfNotNull("", 123, true));
            }
        }

        [Fact]
        public void DictionaryExtensionTests() {
            var dic = new Dictionary<string, string>();
            Assert.Null(dic.AddOrReplace("s1", "a"));
            Assert.Equal("a", dic.AddOrReplace("s1", "b"));
            Assert.Equal("b", dic.AddOrReplace("s1", "a"));
        }

        [Fact]
        public void IEnumerableExtensionTests() {
            IEnumerable<string> list = null;
            Assert.True(list.IsNullOrEmpty());
            Assert.Equal("null", list.ToStringV2((s) => s));

            list = new List<string>();
            Assert.True(list.IsNullOrEmpty());
            Assert.Equal("()", list.ToStringV2((s) => s, bracket1: "(", bracket2: ")"));

            (list as List<string>).Add("s1");
            Assert.False(list.IsNullOrEmpty());
            Assert.Equal("{s1}", list.ToStringV2((s) => s, bracket1: "{", bracket2: "}"));

            (list as List<string>).Add("s2");
            Assert.False(list.IsNullOrEmpty());
            Assert.Equal("[s1, s2]", list.ToStringV2((s) => s, bracket1: "[", bracket2: "]"));
        }

    }
}