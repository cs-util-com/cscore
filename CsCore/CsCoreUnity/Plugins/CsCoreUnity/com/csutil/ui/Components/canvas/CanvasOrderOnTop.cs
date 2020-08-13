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
            var canvas = GetComponent<Canvas>();
            canvas.overrideSorting = true;
            var maxOrderOfAnyCanvasFound = canvas.CalcCurrentMaxSortingOrderInLayer();
            if (maxOrderOfAnyCanvasFound > canvas.sortingOrder) {
                canvas.sortingOrder = maxOrderOfAnyCanvasFound + 1;
            }
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

    }

}
