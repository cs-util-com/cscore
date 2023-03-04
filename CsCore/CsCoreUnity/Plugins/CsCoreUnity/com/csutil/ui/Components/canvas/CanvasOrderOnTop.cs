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

        /// <summary> Can be used to influence the order if many Canvases using a CanvasOrderOnTop are
        /// shown at the same time. E.g toass should be shown on top of Dialogs but both have a CanvasOrderOnTop.
        /// So toasts will use an orderOffset of 20 and Dialogs only 10 so that a shown toast will always be
        /// on top of a dialog shown at the same time </summary>
        public int orderOffset = 1;
        
        private void OnEnable() { SetCanvasSortingOrderOnTop(); }

        private void Start() { SetCanvasSortingOrderOnTop(); }

        private void SetCanvasSortingOrderOnTop() {
            var canvas = gameObject.GetComponentV2<Canvas>();
            var maxOrderOfAnyCanvasFound = canvas.CalcCurrentMaxSortingOrderInLayer();
            // If overrideSorting is already active the sorting order must be at least 1 higher, equal does not work in that case:
            if (canvas.overrideSorting) { maxOrderOfAnyCanvasFound++; }
            if (maxOrderOfAnyCanvasFound >= canvas.sortingOrder) {
                canvas.overrideSorting = true;
                canvas.sortingOrder = maxOrderOfAnyCanvasFound + orderOffset;
            }
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

    }

}
