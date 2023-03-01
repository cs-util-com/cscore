using com.csutil.ui;
using UnityEngine;

namespace com.csutil {

    /// <summary> Can be added to a root canvas to enforce that the <see cref="RootCanvas"/> logic ignores that canvas when collecting all canvases </summary>
    [RequireComponent(typeof(Canvas))]
    public class IgnoreRootCanvas : MonoBehaviour {

#if DEBUG
        private void Start() {
            AssertV2.IsTrue(gameObject.GetComponentV2<Canvas>().isRootCanvasV2(), "IgnoreRootCanvas can only be used on a root canvas");
        }
#endif

    }

}