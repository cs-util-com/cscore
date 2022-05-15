using System;

namespace com.csutil {

    public interface IsDisposable {

        DisposeState IsDisposed { get; }

    }

    public enum DisposeState { Active, DisposingStarted, Disposed }

    public static class DisposeStateHelper {

        public static DisposeState FromBool(bool isDisposed) {
            return isDisposed ? DisposeState.Disposed : DisposeState.Active;
        }

    }

}