using System;

namespace Xunit.Abstractions {

    // Does nothing when injected in a test
    public class ITestOutputHelper {
        public void WriteLine(string debugLogMsg) {
            throw new NotImplementedException();
        }
    }

}