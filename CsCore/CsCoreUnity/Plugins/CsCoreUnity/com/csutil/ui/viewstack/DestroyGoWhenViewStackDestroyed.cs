using UnityEngine;

namespace com.csutil.ui.viewstack {

    /// <summary> Can be used to monitor the viewstack in the children of this gameobject and
    /// destroy this gameobject when the viewstack is destroyed. This is useful e.g. for
    /// cleanups of a parent GO structure placed in 3d space that should disappear when
    /// the viewstack is destroyed. </summary>
    public class DestroyGoWhenViewStackDestroyed : MonoBehaviour, IDisposableV2 {

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;

        private void Start() {
            var viewStack = gameObject.GetComponentInChildren<ViewStack>();
            if (viewStack == null) {
                throw Log.e("Could not find a ViewStack in the children of " + gameObject, gameObject);
            }
            viewStack.gameObject.SetUpDisposeOnDestroy(this);
        }

        public void Dispose() {
            IsDisposed = DisposeState.DisposingStarted;
            gameObject.Destroy(destroyNextFrame: true);
            IsDisposed = DisposeState.Disposed;
        }

    }

}