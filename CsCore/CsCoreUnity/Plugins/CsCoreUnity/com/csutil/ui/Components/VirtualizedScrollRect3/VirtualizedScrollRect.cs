using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Virtualized {

    [Serializable]
    [RequireComponent(typeof(ScrollRect))]
    public abstract class VirtualizedScrollRect<T> : MonoBehaviour where T : MonoBehaviour {

        public enum GrowType { Vertically, Horizontally, }

        public delegate void OnShowItemDelegate(T monoBehaviour, int index);
        public event OnShowItemDelegate OnShowItem;

#pragma warning disable 0649
        [SerializeField] private GrowType growType;
        [SerializeField] private Vector2 cellSize;
        [SerializeField] private Vector2 cellSpacing;

        [Header("Manual Content Fitter Values")]
        [SerializeField] private float bottomPadding;
        [SerializeField] private float rightPadding;

        [SerializeField] private T dataPrefab;
#pragma warning restore 0649

        private RectTransform itemContainer;
        private ScrollRect scrollRect;

        // Used for calculating how far the content is from the top / bottom
        private float topDistance = 0.0f;
        private float bottomDistance = 0.0f;

        private Vector3 itemContainerStartPosition;
        private Vector3 cellStartPosition;
        private Vector3 upperRightBounds;
        private Vector3 lowerLeftBounds;

        private List<T> inactivePool = new List<T>();
        private List<T> activePool = new List<T>();

        public Vector3 contentOffset;

        private int activeTopRow = -1;
        private int activeBottomRow = -1;
        private int activeLeftColumn = -1;
        private int activeRightColumn = -1;

        private float horizontalCenteringOffset = 0.0f;
        private float verticalCenteringOffset = 0.0f;
        private int columnCount;
        private int rowCount;
        private int count;

        private bool isInitialized = false;

        protected ScrollRect ScrollRect { get { return this.scrollRect; } }

        protected float TopDistance { get { return this.topDistance; } }

        protected float BottomDistance { get { return this.bottomDistance; } }

        public void SetCount(int count) {
            this.count = count;
            this.Initialize();
            this.CalculateRowAndColumnCount();

            if (this.growType == GrowType.Vertically) {
                float height = ((this.cellSize.y + this.cellSpacing.y) * this.rowCount) - this.cellSpacing.y;
                this.itemContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            } else {
                float width = ((this.cellSize.x + this.cellSpacing.x) * this.columnCount) - this.cellSpacing.x;
                this.itemContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }

            this.ReplaceAllVisibleTiles();
        }

        public void CenterOnIndex(int index) {
            if (this.count == 0) { return; }

            if (this.growType == GrowType.Vertically) {
                float rowY = this.GetRowY(index);
                float desiredY = (this.lowerLeftBounds.y - this.upperRightBounds.y) / 2.0f;

                this.scrollRect.content.localPosition = AddToY(scrollRect.content.localPosition, desiredY - rowY + (this.cellSize.y / 2.0f));
                this.scrollRect.verticalNormalizedPosition = Mathf.Clamp01(this.scrollRect.verticalNormalizedPosition);
            } else if (this.growType == GrowType.Horizontally) {
                float columnX = this.GetColumnX(index);
                float desiredX = (this.lowerLeftBounds.x - this.upperRightBounds.x) / 2.0f;

                this.scrollRect.content.localPosition = AddToX(scrollRect.content.localPosition, desiredX - columnX + (this.cellSize.x / 2.0f));
                this.scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(this.scrollRect.horizontalNormalizedPosition);
            } else {
                Debug.LogErrorFormat("VirtualizedScrollRect.CenterOnIndex incountered an unknown GrowType {0}", this.growType);
            }
        }

        private static Vector3 AddToX(Vector3 lhs, float val) {
            lhs.x += val;
            return lhs;
        }

        private static Vector3 AddToY(Vector3 lhs, float val) {
            lhs.y += val;
            return lhs;
        }

        /// <summary>
        /// If the Grow Type is vertical, then a negative itemCount will shift contents up, and positive will shift items down.
        /// If the Grow type is horizontal, then a negative itemCount will shift contents right, and positive will shift items left.
        /// </summary>
        public void ShiftContent(int itemCount) {
            if (this.growType == GrowType.Vertically) {
                this.itemContainer.localPosition = AddToY(itemContainer.localPosition, (this.cellSize.y + this.cellSpacing.y) * -itemCount);
            } else {
                this.itemContainer.localPosition = AddToX(itemContainer.localPosition, (this.cellSize.x + this.cellSpacing.x) * -itemCount);
            }
        }

#if UNITY_EDITOR
        public void OnSceneGUI() {
            if (Application.isPlaying) {
                return;
            }

            this.ForceInitialize();
            this.CalculateRowAndColumnCount();

            Matrix4x4 viewportLocalToWorld = this.itemContainer.localToWorldMatrix;
            Vector3 lowerLeftWorldPosition = viewportLocalToWorld.MultiplyPoint(this.lowerLeftBounds);
            Vector3 upperRightWorldPosition = viewportLocalToWorld.MultiplyPoint(this.upperRightBounds);
            this.DrawRect(lowerLeftWorldPosition, upperRightWorldPosition);

            for (int i = 0; i < this.columnCount; i++) {
                for (int j = 0; j < this.rowCount; j++) {
                    Vector3 cellPosition = new Vector3(this.GetColumnX(i), this.GetRowY(j));

                    Matrix4x4 localToWorld = this.itemContainer.localToWorldMatrix;
                    Vector3 lowerLeftCellPosition = localToWorld.MultiplyPoint(cellPosition - (Vector3)(this.cellSize / 2.0f));
                    Vector3 upperRightCellPosition = localToWorld.MultiplyPoint(cellPosition + (Vector3)(this.cellSize / 2.0f));
                    this.DrawRect(lowerLeftCellPosition, upperRightCellPosition);
                }
            }
        }

        private void DrawRect(Vector3 lowerLeft, Vector3 upperRight) {
            Vector3 upperLeft = new Vector3(lowerLeft.x, upperRight.y);
            Vector3 lowerRight = new Vector3(upperRight.x, lowerLeft.y);

            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawLine(lowerLeft, upperLeft);
            UnityEditor.Handles.DrawLine(upperLeft, upperRight);
            UnityEditor.Handles.DrawLine(upperRight, lowerRight);
            UnityEditor.Handles.DrawLine(lowerRight, lowerLeft);
        }
#endif

        protected virtual void Awake() {
            this.Initialize();
            if (this.growType == GrowType.Horizontally) {
                Debug.LogWarning("VirtualizedScrollRect has not been tested yet for Horizontal GrowType. It will probably not work.");
            }
        }

        protected virtual void Update() {
            if (this.count == 0) { return; }

            this.contentOffset = this.GetUpperLeftCornerOfItemContainer() - this.itemContainerStartPosition;

            int newActiveTopRow = 0;
            int newActiveBottomRow = 0;
            int newActiveLeftColumn = 0;
            int newActiveRightColumn = 0;

            this.GetActiveRowsAndColumns(out newActiveTopRow, out newActiveBottomRow, out newActiveLeftColumn, out newActiveRightColumn);

            this.CalculateTopAndBottomDragDistances();

            if (newActiveTopRow == this.activeTopRow && newActiveBottomRow == this.activeBottomRow && newActiveLeftColumn == this.activeLeftColumn && newActiveRightColumn == this.activeRightColumn) {
                return;
            }

            this.activeTopRow = newActiveTopRow;
            this.activeBottomRow = newActiveBottomRow;
            this.activeLeftColumn = newActiveLeftColumn;
            this.activeRightColumn = newActiveRightColumn;

            bool refreshAll = true;

            // TODO [bgish]: Calculate what rows/columns were hidden/shows and only show those
            // TODO [bgish]: If the delta between rows > this.rowCount or delta between columns > this.columnCount, then destroy all children and redraw everything

            if (refreshAll) { this.ReplaceAllVisibleTiles(); }
        }

        private void CalculateTopAndBottomDragDistances() {
            if (this.growType == GrowType.Vertically) {
                // Calculating Top Drag Distance
                float y = this.scrollRect.content.localPosition.y;
                this.topDistance = y > -1 ? 0 : Mathf.Abs(y);

                // Calculating Bottom Drag Distance
                float bottomY = this.scrollRect.content.rect.height - this.scrollRect.content.localPosition.y;
                float scrollerHeight = this.scrollRect.viewport.rect.height;
                float distance = scrollerHeight - bottomY;
                this.bottomDistance = distance > 1 ? distance : 0.0f;
            } else {
                throw new NotImplementedException();
            }
        }

        private Vector3 GetUpperLeftCornerOfItemContainer() {
            Vector3[] itemContainerWorldCorners = new Vector3[4];
            this.itemContainer.GetWorldCorners(itemContainerWorldCorners);
            return this.scrollRect.viewport.worldToLocalMatrix.MultiplyPoint(itemContainerWorldCorners[1]);
        }

        private void ReplaceAllVisibleTiles() {
            this.GetActiveRowsAndColumns(out this.activeTopRow, out this.activeBottomRow, out this.activeLeftColumn, out this.activeRightColumn);

            // putting all active into the deactive pool (aka, moving them slightly off screen)
            Vector3 offscreenLocation = new Vector3(this.GetColumnX(this.activeRightColumn + 2), this.GetRowY(this.activeBottomRow + 2));

            for (int i = 0; i < this.activePool.Count; i++) {
                this.activePool[i].transform.localPosition = offscreenLocation;
                this.inactivePool.Add(this.activePool[i]);
            }

            this.activePool.Clear();

            for (int i = this.activeTopRow; i <= this.activeBottomRow; i++) {
                for (int j = activeLeftColumn; j <= activeRightColumn; j++) {
                    Vector2 position = new Vector2(this.GetColumnX(j), this.GetRowY(i));
                    this.ShowItem(i, j, position);
                }
            }
        }

        private void GetActiveRowsAndColumns(out int topRow, out int bottomRow, out int leftColumn, out int rightColumn) {
            // calculating top/bottom rows
            topRow = Mathf.CeilToInt((this.contentOffset.y - (this.upperRightBounds.y - this.cellStartPosition.y)) / (this.cellSize.y + this.cellSpacing.y));
            bottomRow = Mathf.FloorToInt((this.contentOffset.y + (this.cellStartPosition.y - this.lowerLeftBounds.y)) / (this.cellSize.y + this.cellSpacing.y));

            topRow = Mathf.Max(topRow, 0);
            bottomRow = Mathf.Min(bottomRow, this.rowCount - 1);

            // TODO [bgish]:  We're assuming that all columns are always present.  These really need to be calculated.
            // calculating left/right columns
            leftColumn = 0;
            rightColumn = this.columnCount - 1;
        }

        private void ShowItem(int row, int column, Vector2 localPosition) {
            int index = column + (row * this.columnCount);

            if (index >= this.count) {
                return;
            }

            T newItem = null;

            if (this.inactivePool.Count == 0) {
                newItem = GameObject.Instantiate<T>(this.dataPrefab, this.itemContainer);
            } else {
                int lastIndex = this.inactivePool.Count - 1;
                newItem = this.inactivePool[lastIndex];
                this.inactivePool.RemoveAt(lastIndex);
            }

            this.activePool.Add(newItem);
            Reset(newItem.transform);
            newItem.transform.localPosition = localPosition;

            if (this.OnShowItem != null) {
                this.OnShowItem(newItem, index);
            }
        }

        private static void Reset(Transform transform) {
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        private void ForceInitialize() {
            this.isInitialized = false;
            this.Initialize();
        }

        private void Initialize() {
            if (this.isInitialized) {
                return;
            }

            this.isInitialized = true;

            this.scrollRect = this.GetComponent<ScrollRect>();
            this.itemContainer = this.scrollRect.content;

            Vector3[] itemContainerCorners = new Vector3[4];
            this.itemContainer.GetLocalCorners(itemContainerCorners);
            this.itemContainerStartPosition = this.GetUpperLeftCornerOfItemContainer();

            this.CalculateCellStartAndBounds();

            if (this.growType == GrowType.Horizontally) {
                Debug.LogWarningFormat("GridVirtualizer has not been tested using GrowType {0}", this.growType.ToString());
            }
        }

        private void CalculateCellStartAndBounds() {
            // calculating cell start position
            Vector3[] containerCorners = new Vector3[4];
            this.itemContainer.GetLocalCorners(containerCorners);

            Vector3 startOffset = new Vector3(this.cellSize.x / 2.0f, -this.cellSize.y / 2.0f);
            this.cellStartPosition = containerCorners[1] + startOffset;

            // calculating cell bounds
            Vector3[] viewportCorners = new Vector3[4];
            this.scrollRect.viewport.GetWorldCorners(viewportCorners);

            Vector3 viewportLowerLeft = this.itemContainer.transform.worldToLocalMatrix.MultiplyPoint(viewportCorners[0]);
            Vector3 viewportUpperRight = this.itemContainer.transform.worldToLocalMatrix.MultiplyPoint(viewportCorners[2]);

            Vector3 halfCell = this.cellSpacing + (this.cellSize / 2);
            this.lowerLeftBounds = viewportLowerLeft - halfCell;
            this.upperRightBounds = viewportUpperRight + halfCell;
        }

        private void CalculateRowAndColumnCount() {
            float containerWidth = this.itemContainer.rect.width;
            float containerHeight = this.itemContainer.rect.height;

            this.columnCount = Mathf.FloorToInt((containerWidth + this.cellSpacing.x) / (this.cellSize.x + this.cellSpacing.x));
            this.rowCount = Mathf.FloorToInt((containerHeight + this.cellSpacing.y) / (this.cellSize.y + this.cellSpacing.y));

            // You must always have at least 1 row/column
            this.columnCount = Mathf.Max(1, this.columnCount);
            this.rowCount = Mathf.Max(1, this.rowCount);

            if (this.growType == GrowType.Vertically) {
                this.rowCount = Application.isPlaying ? Mathf.CeilToInt(this.count / (float)this.columnCount) : (this.rowCount + 1);
                this.horizontalCenteringOffset = (containerWidth - ((this.cellSize.x + this.cellSpacing.x) * this.columnCount) + this.cellSpacing.x) / 2.0f;
                this.verticalCenteringOffset = 0.0f;
            } else {
                this.columnCount = Application.isPlaying ? Mathf.CeilToInt(this.count / (float)this.rowCount) : (this.columnCount + 1);
                this.horizontalCenteringOffset = 0.0f;
                this.verticalCenteringOffset = (containerHeight - ((this.cellSize.y + this.cellSpacing.y) * this.rowCount) + this.cellSpacing.y) / 2.0f;
            }
        }

        private float GetColumnX(int columnIndex) {
            return this.cellStartPosition.x + this.horizontalCenteringOffset + ((this.cellSize.x + this.cellSpacing.x) * columnIndex);
        }

        private float GetRowY(int rowIndex) {
            return this.cellStartPosition.y - this.verticalCenteringOffset - ((this.cellSize.y + this.cellSpacing.y) * rowIndex);
        }

    }

}