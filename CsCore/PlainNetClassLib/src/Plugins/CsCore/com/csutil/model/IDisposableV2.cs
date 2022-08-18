using System;

namespace com.csutil {

    /// <summary> Extends <see cref="IDisposable"/> to also implement an <see cref="IsDisposed"/> state </summary>
    public interface IDisposableV2 : IDisposable {

        /// <summary> Can be used to check if the object is disposed or alive </summary>
        DisposeState IsDisposed { get; }

    }

    public enum DisposeState { Active, DisposingStarted, Disposed }

    public static class DisposeStateHelper {

        public static DisposeState FromBool(bool isDisposed) {
            return isDisposed ? DisposeState.Disposed : DisposeState.Active;
        }

    }

}