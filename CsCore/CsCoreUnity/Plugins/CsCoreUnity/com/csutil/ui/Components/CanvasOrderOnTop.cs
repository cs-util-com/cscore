using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    /// <summary>
    /// Ensures that the sorting order is set correctly when the canvas gets enabled and also that 
    /// a GraphicsRaycaster is present so that the nested canvas still can receive input events
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    class CanvasOrderOnTop : MonoBehaviour {

        private void OnEnable() { SetCanvasSortingOrderOnTop(); }

        private void Start() { SetCanvasSortingOrderOnTop(); }

        private void SetCanvasSortingOrderOnTop() {
            var canvas = GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = canvas.CalcCurrentMaxSortingOrderInLayer() + 1;
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

    }

}
