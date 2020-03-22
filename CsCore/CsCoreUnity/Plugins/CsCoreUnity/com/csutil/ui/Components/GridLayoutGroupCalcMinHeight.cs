using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil {

    /// <summary> Automatically calculates the minimum needed height for a grid layout based on its content </summary>
    [RequireComponent(typeof(GridLayoutGroup), typeof(LayoutElement))]
    public class GridLayoutGroupCalcMinHeight : MonoBehaviour {

        private void OnEnable() { this.ExecuteRepeated(UpdateHeight, 100, 100); }

        private void OnValidate() { UpdateHeight(); }

        private bool UpdateHeight() {
            if (!enabled) { return false; }
            var grid = GetComponent<GridLayoutGroup>();
            if (grid.constraint == GridLayoutGroup.Constraint.Flexible) {
                var activeChildCount = grid.gameObject.GetChildrenIEnumerable().Filter(c => c.activeSelf).Count();

                // From CalculateLayoutInputVertical() in https://bitbucket.org/Unity-Technologies/ui/src/2019.1/UnityEngine.UI/UI/Core/Layout/GridLayoutGroup.cs
                float width = (grid.transform as RectTransform).rect.width;
                int cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - grid.padding.horizontal + grid.spacing.x + 0.001f) / (grid.cellSize.x + grid.spacing.x)));
                var minRows = Mathf.CeilToInt(activeChildCount / (float)cellCountX);
                var calculatedMinRequiredHeight = grid.padding.vertical + (grid.cellSize.y + grid.spacing.y) * minRows - grid.spacing.y;
                var ele = GetComponent<LayoutElement>();
                if (calculatedMinRequiredHeight != ele.minHeight) { ele.minHeight = calculatedMinRequiredHeight; }
            } else {
                Log.e("GridLayoutGroupAutoHeight only works with GridLayoutGroup.Constraint.Flexible for now");
            }
            return true;
        }
    }

}