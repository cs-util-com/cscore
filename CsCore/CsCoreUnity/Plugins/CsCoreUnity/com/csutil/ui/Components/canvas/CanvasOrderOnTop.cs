using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    /// <summary>
    /// Ensures that the sorting order is set correctly when the canvas gets enabled and also that 
    /// a GraphicsRaycaster is present so that the nested canvas still can receive input events
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    class CanvasOrderOnTop : MonoBehaviour {

        /// <summary> If true canvas.CalcCurrentMaxSortingOrderInLayer will not
        /// include this canvas in the calculation of the max order </summary>
        public bool excludeFromOrderCalc = false;

        private void OnEnable() { SetCanvasSortingOrderOnTop(); }

        private void Start() { SetCanvasSortingOrderOnTop(); }

        private void SetCanvasSortingOrderOnTop() {
            var canvas = gameObject.GetComponentV2<Canvas>();
            var maxOrderOfAnyCanvasFound = canvas.CalcCurrentMaxSortingOrderInLayer();
            // If overrideSorting is already active the sorting order must be at least 1 higher, equal does not work in that case:
            if (canvas.overrideSorting) { maxOrderOfAnyCanvasFound++; }
            if (maxOrderOfAnyCanvasFound >= canvas.sortingOrder) {
                canvas.overrideSorting = true;
                canvas.sortingOrder = maxOrderOfAnyCanvasFound + 1;
            }
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

    }

}
