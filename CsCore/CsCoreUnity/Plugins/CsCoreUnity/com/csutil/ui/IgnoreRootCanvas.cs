using System.Diagnostics;
using com.csutil.ui;
using UnityEngine;

namespace com.csutil {

    /// <summary> Can be added to a canvas to enforce that the <see cref="RootCanvas"/> logic ignores that canvas when collecting all canvases </summary>
    [RequireComponent(typeof(Canvas))]
    public class IgnoreRootCanvas : MonoBehaviour {

        private void Start() {
            AssertIsInRootCanvas();
        }

        [Conditional("DEBUG")]
        private void AssertIsInRootCanvas() {
            if (!gameObject.GetComponentV2<Canvas>().isRootCanvasV2()) {
                Log.e("IgnoreRootCanvas can only be used on a root canvas", gameObject);
            }
        }

    }

}