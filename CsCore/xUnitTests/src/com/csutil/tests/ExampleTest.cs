using System;

namespace com.csutil.tests {

    public abstract class DefaultTest : IDisposable {

        static DefaultTest() {
            // set assert to throws only once at beginning since tests are executed in parallel:
            AssertV2.throwExeptionIfAssertionFails = true;
        }

        public DefaultTest() { SetupBeforeEachTest(); }

        public void Dispose() { TearDownAfterEachTest(); }

        public virtual void SetupBeforeEachTest() { }

        public virtual void TearDownAfterEachTest() { }

    }

}