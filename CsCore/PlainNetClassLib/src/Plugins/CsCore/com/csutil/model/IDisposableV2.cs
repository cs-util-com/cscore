using System;
using System.Collections.Generic;
using com.csutil.keyvaluestore;

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

    public class IDisposableCollection : IDisposableV2 {

        public DisposeState IsDisposed { get; private set; }
        public ICollection<IDisposable> Children { get; }

        public IDisposableCollection() {
            Children = new List<IDisposable>();
        }

        public IDisposableCollection(ICollection<IDisposable> children) {
            Children = children;
        }

        public void Dispose() {
            this.ThrowErrorIfDisposed();
            IsDisposed = DisposeState.DisposingStarted;
            foreach (var child in Children) {
                try {
                    child.Dispose();
                } catch (Exception e) {
                    Log.e(e);
                }
            }
            IsDisposed = DisposeState.Disposed;
        }

        public void Add(IDisposable disposable) {
            Children.Add(disposable);
        }

    }

}