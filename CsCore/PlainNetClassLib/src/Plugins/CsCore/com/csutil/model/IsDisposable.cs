using System;

namespace com.csutil {

    public interface IsDisposable {

        enum State { Active, DisposingStarted, Disposed }

        State IsDisposed { get; }

        public static State StateFromBool(bool isDisposed) { return isDisposed ? State.Disposed : State.Active; }
        
    }

}