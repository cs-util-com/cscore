using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.TestTools;
using Xunit;

namespace com.csutil.tests {

    public class AssertTests {

        private void Throws(Action a) {
            var noException = false;
            try { a(); noException = true; } catch (Exception) { }
            if (noException) { throw Log.e("No exception thrown"); }
        }

        [NUnit.Framework.Test]
        public void TestThatAssertClassWorksCorrectly() {

            Throws(() => { Assert.True(false); });
            Throws(() => { Assert.False(true); });

            Throws(() => { Assert.Null(""); });
            Throws(() => { Assert.NotNull<string>(null); });

            Assert.Equal(4, 4);
            Throws(() => { Assert.Equal(3, 1 + 1); });
            Throws(() => { Assert.NotEqual(2, 1 + 1); });

            Assert.IsType<string>("");
            Throws(() => { Assert.IsType<string>(1); });

            Assert.Throws<FormatException>(() => { throw new FormatException(); });
            Throws(() => { Assert.Throws<FormatException>(() => { throw new Exception(); }); });

            var o1 = new object();
            var o2 = new object();
            Throws(() => { Assert.Same(o1, o2); });
            o2 = o1;
            Throws(() => { Assert.NotSame(o1, o2); });

            Assert.InRange(2, min: 2, max: 4);
            Assert.InRange(3, min: 2, max: 4);
            Assert.InRange(4f, min: 2, max: 4);
            Throws(() => { Assert.InRange(5, min: 2, max: 4); });

            var l = new List<string>();
            Throws(() => { Assert.NotEmpty(l); });
            Assert.Empty(l);
            l.Add("a");
            Assert.NotEmpty(l);
            Throws(() => { Assert.Empty(l); });
            Assert.Single(l);
            l.Add("b");
            Throws(() => { Assert.Single(l); });
            Assert.Contains("a", l);
            Throws(() => { Assert.Contains("c", l); });

        }

        [UnityTest]
        public IEnumerator TestThrowsAsyncPart1() {
            yield return Assert.ThrowsAsync<FormatException>(async () => {
                await TaskV2.Delay(10);
                throw new FormatException();
            }).AsCoroutine();
            yield return TaskV2.Run(TestThrowsAsyncPart2).AsCoroutine();
        }

        private async Task TestThrowsAsyncPart2() {
            try {
                await Assert.ThrowsAsync<FormatException>(async () => {
                    await TaskV2.Delay(10);
                    throw new NullReferenceException("abc");
                });
                throw Log.e("Assert.ThrowsAsync did not rethrow the NullReferenceException");
            }
            catch (NullReferenceException e) { NUnit.Framework.Assert.AreEqual("abc", e.Message); }
        }

    }

}