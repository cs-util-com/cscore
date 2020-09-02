using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    /// <summary> Initial version MIT Licensed from https://github.com/aillieo/UnityDynamicScrollView </summary>
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class VirtualizedScrollRect : ScrollRect {

        private class ScrollItemWithRect {

            // scroll item RectTransform component on the body
            public RectTransform item;

            // scroll item position in scrollview
            public Rect rect;

            // true if rect needs to update
            public bool rectDirty = true;
        }

        int m_dataCount = 0;
        List<ScrollItemWithRect> managedItems = new List<ScrollItemWithRect>();

        // for hide and show
        public enum ItemLayoutType {
            // The last digit indicates the scroll direction
            Vertical = 1,                   // 0001
            Horizontal = 2,                 // 0010
            VerticalThenHorizontal = 4,     // 0100
            HorizontalThenVertical = 5,     // 0101
        }
        const int flagScrollDirection = 1;  // 0001


        [SerializeField]
        ItemLayoutType m_layoutType = ItemLayoutType.Vertical;
        ItemLayoutType layoutType { get { return m_layoutType; } }


        // const int instead of enum to reduce (int) and (CriticalItemType) conversion
        static class CriticalItemType {
            public const int UpToHide = 0;
            public const int DownToHide = 1;
            public const int UpToShow = 2;
            public const int DownToShow = 3;
        }
        // Only save 4 critical indexes
        int[] criticalItemIndex = new int[4];
        Rect refRect;

        // Current moving direction
        Vector2 m_prevPosition;
        Vector2 m_curDelta;

        // resource management
        SimpleObjPool<RectTransform> itemPool = null;

        [Tooltip("Number of items in the pool at initialization")]
        public int poolSize;

        [Tooltip("Default item size")]
        public Vector2 defaultItemSize;

        [Tooltip("item template")]
        public RectTransform itemTemplate;

        // callbacks for items
        public Action<int, RectTransform> updateFunc;
        public Func<int, Vector2> itemSizeFunc;
        public Func<int> itemCountFunc;
        public Func<int, RectTransform> itemGetFunc;
        public Action<RectTransform> itemRecycleFunc;

        // status
        private bool initialized = false;
        private bool willUpdateData = false;

        public void SetUpdateFunc(Action<int, RectTransform> func) {
            updateFunc = func;
        }

        public void SetItemSizeFunc(Func<int, Vector2> func) {
            itemSizeFunc = func;
        }

        public void SetItemCountFunc(Func<int> func) {
            itemCountFunc = func;
        }

        public void SetItemGetAndRecycleFunc(Func<int, RectTransform> getFunc, Action<RectTransform> recycleFunc) {
            if (getFunc != null && recycleFunc != null) {
                itemGetFunc = getFunc;
                itemRecycleFunc = recycleFunc;
            }
        }

        public void UpdateData(bool immediately = true) {
            if (!initialized) { InitScrollView(); }
            if (immediately) {
                InternalUpdateData();
            } else {
                if (!willUpdateData) {
                    willUpdateData = true;
                    StartCoroutine(DelayUpdateData());
                }
            }
        }

        public void ScrollTo(int index) {
            index = Mathf.Clamp(index, 0, m_dataCount - 1);
            EnsureItemRect(index);
            Rect r = managedItems[index].rect;
            int dir = (int)layoutType & flagScrollDirection;
            if (dir == 1) { // vertical
                float value = 1 - (-r.yMax / (content.sizeDelta.y - refRect.height));
                value = Mathf.Clamp01(value);
                SetNormalizedPosition(value, 1);
            } else { // horizontal
                float value = r.xMin / (content.sizeDelta.x - refRect.width);
                value = Mathf.Clamp01(value);
                SetNormalizedPosition(value, 0);
            }
        }

        private IEnumerator DelayUpdateData() {
            yield return null;
            InternalUpdateData();
        }

        private void InternalUpdateData() {
            willUpdateData = false;

            int newDataCount = 0;
            if (itemCountFunc != null) { newDataCount = itemCountFunc(); }

            if (newDataCount != managedItems.Count) {
                if (managedItems.Count < newDataCount) { //increase
                    foreach (var itemWithRect in managedItems) {
                        // Reset all rects
                        itemWithRect.rectDirty = true;
                    }
                    while (managedItems.Count < newDataCount) {
                        managedItems.Add(new ScrollItemWithRect());
                    }
                } else { //Reduce, reserve space, avoid GC
                    for (int i = 0, count = managedItems.Count; i < count; ++i) {
                        // Reset all rects
                        managedItems[i].rectDirty = true;

                        // Excess part Clean up and recycle item
                        if (i >= newDataCount) {
                            if (managedItems[i].item != null) {
                                RecycleOldItem(managedItems[i].item);
                                managedItems[i].item = null;
                            }
                        }
                    }
                }
            }

            m_dataCount = newDataCount;
            ResetCriticalItems();
        }

        void ResetCriticalItems() {
            bool hasItem, shouldShow;
            int firstIndex = -1, lastIndex = -1;

            for (int i = 0; i < m_dataCount; i++) {
                hasItem = managedItems[i].item != null;
                shouldShow = ShouldItemSeenAtIndex(i);

                if (shouldShow) {
                    if (firstIndex == -1) { firstIndex = i; }
                    lastIndex = i;
                }

                if (hasItem && shouldShow) { // Should be displayed 
                    SetDataForItemAtIndex(managedItems[i].item, i);
                    continue;
                }

                if (hasItem == shouldShow) { // Should not be displayed 
                    // Already traversed all items to be displayed, skip latter ones:
                    if (firstIndex != -1) { break; }
                    continue;
                }

                if (hasItem && !shouldShow) {
                    RecycleOldItem(managedItems[i].item);
                    managedItems[i].item = null;
                    continue;
                }

                if (shouldShow && !hasItem) {
                    RectTransform item = GetNewItem(i);
                    OnGetItemForDataIndex(item, i);
                    managedItems[i].item = item;
                    continue;
                }

            }

            // content.localPosition = Vector2.zero;
            criticalItemIndex[CriticalItemType.UpToHide] = firstIndex;
            criticalItemIndex[CriticalItemType.DownToHide] = lastIndex;
            criticalItemIndex[CriticalItemType.UpToShow] = Mathf.Max(firstIndex - 1, 0);
            criticalItemIndex[CriticalItemType.DownToShow] = Mathf.Min(lastIndex + 1, m_dataCount - 1);
        }

        protected override void SetContentAnchoredPosition(Vector2 position) {
            base.SetContentAnchoredPosition(position);
            m_curDelta = content.anchoredPosition - m_prevPosition;
            m_prevPosition = content.anchoredPosition;
            UpdateCriticalItems();
        }

        protected override void SetNormalizedPosition(float value, int axis) {
            base.SetNormalizedPosition(value, axis);
            ResetCriticalItems();
        }

        RectTransform GetCriticalItem(int type) {
            int index = criticalItemIndex[type];
            if (index >= 0 && index < m_dataCount) { return managedItems[index].item; }
            return null;
        }


        bool IsCriticalItemTypeValid(int type) {
            int dir = (int)layoutType & flagScrollDirection;

            if (dir == 1) {
                if (m_curDelta[dir] > 0) {
                    return type == CriticalItemType.UpToHide || type == CriticalItemType.DownToShow;
                } else if (m_curDelta[dir] < 0) {
                    return type == CriticalItemType.DownToHide || type == CriticalItemType.UpToShow;
                }
            } else { // dir == 0
                if (m_curDelta[dir] < 0) {
                    return type == CriticalItemType.UpToHide || type == CriticalItemType.DownToShow;
                } else if (m_curDelta[dir] > 0) {
                    return type == CriticalItemType.DownToHide || type == CriticalItemType.UpToShow;
                }
            }
            return false;
        }


        void UpdateCriticalItems() {
            //Debug.LogWarning((m_curDelta.y > 0 ? "↑↑" : "↓↓") + " criticalItemIndex = {" + criticalItemIndex[0] + " " + criticalItemIndex[1] + " " + criticalItemIndex[2] + " " + criticalItemIndex[3] + "}");

            for (int i = CriticalItemType.UpToHide; i <= CriticalItemType.DownToShow; i++) {
                if (!IsCriticalItemTypeValid(i)) { continue; }

                if (i <= CriticalItemType.DownToHide) { // Hide items that leave the visible area
                    CheckAndHideItem(i);
                } else { // Display items that enter the visible area
                    CheckAndShowItem(i);
                }
            }
        }


        void CheckAndHideItem(int criticalItemType) {
            RectTransform item = null;
            int criticalIndex = -1;
            while (true) {
                item = GetCriticalItem(criticalItemType);
                criticalIndex = criticalItemIndex[criticalItemType];
                if (item != null && !ShouldItemSeenAtIndex(criticalIndex)) {
                    RecycleOldItem(item);
                    managedItems[criticalIndex].item = null;
                    //Debug.Log("Recycled " + criticalIndex);
                    criticalItemIndex[criticalItemType + 2] = criticalIndex;
                    if (criticalItemType == CriticalItemType.UpToHide) { // The top hidden one
                        criticalItemIndex[criticalItemType]++;
                    } else { // Hidden one at the bottom
                        criticalItemIndex[criticalItemType]--;
                    }
                    criticalItemIndex[criticalItemType] = Mathf.Clamp(criticalItemIndex[criticalItemType], 0, m_dataCount - 1);
                } else {
                    break;
                }

            }
        }


        void CheckAndShowItem(int criticalItemType) {
            RectTransform item = null;
            int criticalIndex = -1;

            while (true) {
                item = GetCriticalItem(criticalItemType);
                criticalIndex = criticalItemIndex[criticalItemType];

                //if (item == null && ShouldItemFullySeenAtIndex(criticalItemIndex[criticalItemType - 2]))

                if (item == null && ShouldItemSeenAtIndex(criticalIndex)) {
                    RectTransform newItem = GetNewItem(criticalIndex);
                    OnGetItemForDataIndex(newItem, criticalIndex);
                    //Debug.Log("created " + criticalIndex);
                    managedItems[criticalIndex].item = newItem;

                    criticalItemIndex[criticalItemType - 2] = criticalIndex;

                    if (criticalItemType == CriticalItemType.UpToShow) { // The top one
                        criticalItemIndex[criticalItemType]--;
                    } else { // The bottom one
                        criticalItemIndex[criticalItemType]++;
                    }
                    criticalItemIndex[criticalItemType] = Mathf.Clamp(criticalItemIndex[criticalItemType], 0, m_dataCount - 1);
                } else {
                    break;
                }
            }
        }


        bool ShouldItemSeenAtIndex(int index) {
            if (index < 0 || index >= m_dataCount) { return false; }
            EnsureItemRect(index);
            return new Rect(refRect.position - content.anchoredPosition, refRect.size).Overlaps(managedItems[index].rect);
        }

        //bool ShouldItemFullySeenAtIndex(int index) {
        //    if (index < 0 || index >= m_dataCount) { return false; }
        //    EnsureItemRect(index);
        //    return IsRectContains(new Rect(refRect.position - content.anchoredPosition, refRect.size), (managedItems[index].rect));
        //}

        //bool IsRectContains(Rect outRect, Rect inRect, bool bothDimensions = false) {
        //    if (bothDimensions) {
        //        bool xContains = (outRect.xMax >= inRect.xMax) && (outRect.xMin <= inRect.xMin);
        //        bool yContains = (outRect.yMax >= inRect.yMax) && (outRect.yMin <= inRect.yMin);
        //        return xContains && yContains;
        //    } else {
        //        int dir = (int)layoutType & flagScrollDirection;
        //        if (dir == 1) {
        //            // Vertical scroll only y direction
        //            return (outRect.yMax >= inRect.yMax) && (outRect.yMin <= inRect.yMin);
        //        } else // = 0
        //          {
        //            // Scroll horizontally only calculate the x direction
        //            return (outRect.xMax >= inRect.xMax) && (outRect.xMin <= inRect.xMin);
        //        }
        //    }
        //}

        void InitPool() {
            GameObject poolNode = new GameObject("POOL");
            poolNode.SetActive(false);
            poolNode.transform.SetParent(transform, false);
            itemPool = new SimpleObjPool<RectTransform>(
                poolSize,
                (RectTransform item) => {
                    item.transform.SetParent(poolNode.transform, false);
                },
                () => {
                    GameObject itemObj = Instantiate(itemTemplate.gameObject);
                    RectTransform item = itemObj.GetComponent<RectTransform>();
                    itemObj.transform.SetParent(poolNode.transform, false);

                    item.anchorMin = Vector2.up;
                    item.anchorMax = Vector2.up;
                    item.pivot = Vector2.zero;
                    //rectTrans.pivot = Vector2.up;

                    itemObj.SetActive(true);
                    return item;
                });
        }

        void OnGetItemForDataIndex(RectTransform item, int index) {
            SetDataForItemAtIndex(item, index);
            item.transform.SetParent(content, false);
        }

        void SetDataForItemAtIndex(RectTransform item, int index) {
            updateFunc?.Invoke(index, item);
            SetPosForItemAtIndex(item, index);
        }

        void SetPosForItemAtIndex(RectTransform item, int index) {
            EnsureItemRect(index);
            Rect r = managedItems[index].rect;
            item.localPosition = r.position;
            item.sizeDelta = r.size;
        }

        Vector2 GetItemSize(int index) {
            if (itemSizeFunc != null && index >= 0 && index <= m_dataCount) { return itemSizeFunc(index); }
            return defaultItemSize;
        }

        private RectTransform GetNewItem(int index) {
            RectTransform item;
            if (itemGetFunc != null) {
                item = itemGetFunc(index);
            } else {
                item = itemPool.Get();
            }
            return item;
        }

        private void RecycleOldItem(RectTransform item) {
            if (itemRecycleFunc != null) {
                itemRecycleFunc(item);
            } else {
                itemPool.Recycle(item);
            }
        }

        void InitScrollView() {
            initialized = true;

            // Control the scrolling direction of the original ScrollRect according to the settings
            int dir = (int)layoutType & flagScrollDirection;
            vertical = (dir == 1);
            horizontal = (dir == 0);

            content.pivot = Vector2.up;
            InitPool();
            UpdateRefRect();

            m_curDelta = content.anchoredPosition - m_prevPosition;
            m_prevPosition = content.anchoredPosition;
        }

        Vector3[] viewWorldConers = new Vector3[4];
        Vector3[] rectCorners = new Vector3[2];
        void UpdateRefRect() {
            /*
             *  WorldCorners
             * 
             *    1 ------- 2     
             *    |         |
             *    |         |
             *    0 ------- 3
             * 
             */

            // refRect is the rect of the viewport under the Content node
            viewRect.GetWorldCorners(viewWorldConers);
            rectCorners[0] = content.transform.InverseTransformPoint(viewWorldConers[0]);
            rectCorners[1] = content.transform.InverseTransformPoint(viewWorldConers[2]);
            refRect = new Rect((Vector2)rectCorners[0] - content.anchoredPosition, rectCorners[1] - rectCorners[0]);
        }

        void MovePos(ref Vector2 pos, Vector2 size) {
            // Note that all rects are based on the lower left corner
            switch (layoutType) {
                case ItemLayoutType.Vertical: // Vertically move down
                    pos.y -= size.y;
                    break;
                case ItemLayoutType.Horizontal: // Move horizontally to the right
                    pos.x += size.x;
                    break;
                case ItemLayoutType.VerticalThenHorizontal:
                    pos.y -= size.y;
                    if (pos.y <= -refRect.height) {
                        pos.y = 0;
                        pos.x += size.x;
                    }
                    break;
                case ItemLayoutType.HorizontalThenVertical:
                    pos.x += size.x;
                    if (pos.x >= refRect.width) {
                        pos.x = 0;
                        pos.y -= size.y;
                    }
                    break;
            }
        }

        void EnsureItemRect(int index) {
            if (!managedItems[index].rectDirty) { return; } // Already clean

            ScrollItemWithRect firstItem = managedItems[0];
            if (firstItem.rectDirty) {
                Vector2 firstSize = GetItemSize(0);
                firstItem.rect = CreateWithLeftTopAndSize(Vector2.zero, firstSize);
                firstItem.rectDirty = false;
            }

            // The most recently updated rect before the current item
            int nearestClean = 0;
            for (int i = index; i >= 0; --i) {
                if (!managedItems[i].rectDirty) {
                    nearestClean = i;
                    break;
                }
            }

            // Need to update the size from nearestClean to index
            Rect nearestCleanRect = managedItems[nearestClean].rect;
            Vector2 curPos = GetLeftTop(nearestCleanRect);
            Vector2 size = nearestCleanRect.size;
            MovePos(ref curPos, size);

            for (int i = nearestClean + 1; i <= index; i++) {
                size = GetItemSize(i);
                managedItems[i].rect = CreateWithLeftTopAndSize(curPos, size);
                managedItems[i].rectDirty = false;
                MovePos(ref curPos, size);
            }
            Vector2 range = new Vector2(Mathf.Abs(curPos.x), Mathf.Abs(curPos.y));
            switch (layoutType) {
                case ItemLayoutType.VerticalThenHorizontal:
                    range.x += size.x;
                    range.y = refRect.height;
                    break;
                case ItemLayoutType.HorizontalThenVertical:
                    range.x = refRect.width;
                    if (curPos.x != 0) {
                        range.y += size.y;
                    }
                    break;
            }
            content.sizeDelta = range;
        }

        private static Vector2 GetLeftTop(Rect rect) {
            Vector2 ret = rect.position;
            ret.y += rect.size.y;
            return ret;
        }

        private static Rect CreateWithLeftTopAndSize(Vector2 leftTop, Vector2 size) {
            Vector2 leftBottom = leftTop - new Vector2(0, size.y);
            return new Rect(leftBottom, size);
        }

        protected override void OnDestroy() {
            if (itemPool != null) {
                itemPool.Purge();
            }
        }

    }

}