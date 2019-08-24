//  
// Copyright (c) 2019 Shane Harper
// Licensed under the MIT. See LICENSE file full license information.  
//  

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Sharper.Scroller
{
    public abstract partial class Scroller<TAsset, TData> : MonoBehaviour,
        IBeginDragHandler, IEndDragHandler, IDragHandler, IScroller<TData>
        where TAsset : MonoBehaviour, IScrollerPrefab<TData>
        where TData : IScrollerData
    {
        private Dictionary<int, TAsset> _activeAssets;
        private IList<TData> _data;
        private Pool _pool;
        private Vector2 _viewportSize;
        private Vector2 _prefabSize;
        private Vector2 _itemOffset;
        private Vector2 _zoomOffset;
        private int _loopPadding;
        
        public float VirtualSize { get; private set; }
        public int ItemCount { get; private set; }
        protected float NormalizedPosition { get; private set; }
        
        public bool IsInitialized
        {
            get {  return _data != null; }
        }

        public ScrollerEvent OnValueChanged
        {
            get { return _onValueChanged; }
        }

        public TData CurrentItem
        {
            get { return _data[CurrentIndex]; }
        }

        public int CurrentIndex
        {
            get
            {
                var drag = _dragVelocity * Time.unscaledDeltaTime * _scrollIntertia;
                var value = Mathf.RoundToInt(ItemCount * NormalizedPosition + drag);
                
                if (_loop) value %= ItemCount;
                else value = Mathf.Clamp(value, 0, ItemCount - 1);
                
                return _reverse ? ItemCount - value - 1 : value;
            }
        }

        public virtual void SetItems(IList<TData> data)
        {
            // Clean up existing active assets (if applicable)
            if (_activeAssets != null)
                ReturnAllAssetsToPool(false);
            else
                _activeAssets = new Dictionary<int, TAsset>(4);

            ItemCount = data.Count;
            _data = data;
            _pool = new Pool(_prefab, _viewport);

            // Update cached values
            RefreshCachedValues();
            JumpTo(CurrentIndex);
        }

        public virtual void JumpTo(int index)
        {
            if (_reverse) index = ItemCount - index - 1;
                
            StopScroll();
            var value = (float) index / ItemCount % 1;
            NormalizedPosition = value;
            Redraw();

            _onValueChanged.Invoke(index);
        }

        public virtual void ScrollTo(int index)
        {
            // If not active, fall back to JumpTo
            if (!gameObject.activeInHierarchy)
            {
                JumpTo(index);
                return;
            }
            
            if (_reverse) index = ItemCount - index - 1;
            
            var scrollValue = (float) index / ItemCount % 1;
            if (_scrollAnimation != null) StopCoroutine(_scrollAnimation);
            _scrollAnimation = StartCoroutine(AnimateTo(scrollValue, _scrollAnimationSpeed, _loop));
            
            _onValueChanged.Invoke(index); 
        }

        /// <summary>
        ///     Redraw the scroller view
        /// </summary>
        public virtual void Redraw()
        {
            var min = 0;
            var max = ItemCount;
            if (_loop)
            {
                // Draw a minimum number when looping
                min = -_loopPadding;
                max += _loopPadding;

                NormalizedPosition %= 1;
                while (NormalizedPosition < 0) ++NormalizedPosition;
            }
            
            for (var i = min; i < max; ++i)
            {
                var position = GetPosition(i);
                var scale = _zoomCurve.Evaluate(1f - Vector2.Distance(_itemOffset, position) / _zoomArea);
                var isVisible = IsVisible(position, scale);

                var offset = Vector2.Scale(_prefabSize * (scale-1), _zoomOffset);
                position += offset;

                TAsset asset;
                var assetExists = _activeAssets.TryGetValue(i, out asset);

                if (isVisible)
                {
                    var isNew = false;
                    if (!assetExists)
                    {
                        bool instantiated;
                        asset = _pool.Get(out instantiated);
                        if (instantiated) OnNewAssetCreated(asset);
                        
                        asset.gameObject.SetActive(true);
                        _activeAssets[i] = asset;

                        var x = i % ItemCount;
                        while (x < 0) x += ItemCount;
                        if (_reverse) x = ItemCount - x - 1;
                            
                        asset.SetData(x, _data[x]);
                        isNew = true;
                    }

                    // Apply new anchored position
                    asset.RectTransform.anchoredPosition = position;

                    // Apply scale change
                    asset.RectTransform.localScale = Vector3.one * scale;

                    // Enable gameObject last if it is new
                    if (isNew) asset.gameObject.SetActive(true);
                }
                else if (assetExists)
                {
                    _activeAssets.Remove(i);
                    if (_setInactiveInPool) asset.gameObject.SetActive(false);
                    _pool.Return(asset);
                }
            }
        }
        
        /// <summary>
        ///     Refreshes cached values and returns all existing assets to the pool before redrawing
        /// </summary>
        /// <remarks>This method is expensive, avoid using unless necessary</remarks>
        public void HardRedraw(bool destroyPool)
        {
            RefreshCachedValues();
            ReturnAllAssetsToPool(destroyPool);
            Redraw();
        }
        
        /// <summary>
        ///     Called whenever a new asset is created instead of drawing from the pool
        /// </summary>
        /// <param name="asset">The new asset</param>
        protected abstract void OnNewAssetCreated(TAsset asset);

        #region Utility
        
        private void RefreshCachedValues()
        {
            VirtualSize = ItemCount * _spacing;
            _prefabSize = _prefab != null ? _prefab.RectTransform.rect.size : Vector2.zero;
            _viewportSize = _viewport != null ? _viewport.rect.size : Vector2.zero;
            _itemOffset = GetAlignmentOffset(_alignment, _prefabSize, _viewportSize, _padding);
            _zoomOffset = GetZoomOffset(_alignment, _scrollAxis);

            var maxScrollSpace = _viewportSize + _prefabSize;
            var scrollSize = _scrollAxis == RectTransform.Axis.Vertical ? maxScrollSpace.y : maxScrollSpace.x;
            _loopPadding = Mathf.CeilToInt(scrollSize / _spacing * 0.5f);
        }
        
        private void ReturnAllAssetsToPool(bool destroy)
        {
            foreach (var asset in _activeAssets.Values)
            {
                if (asset == null) continue;
                if (destroy)
                {
                    Destroy(asset);
                }
                else
                {
                    if (_setInactiveInPool) asset.gameObject.SetActive(false);
                    _pool.Return(asset);
                }
            }
            _activeAssets.Clear();
        }
        
        private Vector2 GetPosition(int index)
        {
            var position = _spacing * index - VirtualSize * NormalizedPosition;
            
            if (_scrollAxis == RectTransform.Axis.Vertical)
                return _itemOffset + new Vector2(0, position);
            return _itemOffset + new Vector2(position, 0);
        }
        
        private bool IsVisible(Vector2 position, float scale)
        {
            var safeZone = (_prefabSize * scale + _viewportSize) * 0.5f;
            return position.x <= safeZone.x && position.x >= -safeZone.x &&
                   position.y <= safeZone.y && position.y >= -safeZone.y;
        }
        
        private static Vector2 GetAlignmentOffset(TextAnchor alignment, Vector2 prefabSize, Vector2 viewPortSize, RectOffset padding)
        {
            var d = (viewPortSize - prefabSize) * 0.5f;
            switch (alignment)
            {
                case TextAnchor.UpperLeft:
                    return new Vector2(-d.x + padding.left, d.y - padding.top);
                case TextAnchor.UpperRight:
                    return new Vector2(d.x - padding.right, d.y - padding.top);
                case TextAnchor.UpperCenter:
                    return new Vector2(0, d.y - padding.top);
                case TextAnchor.MiddleLeft:
                    return new Vector2(-d.x + padding.left, 0); 
                case TextAnchor.MiddleRight:
                    return new Vector2(d.x - padding.right, 0);
                case TextAnchor.LowerLeft:
                    return new Vector2(-d.x + padding.left, -d.y + padding.bottom);
                case TextAnchor.LowerRight:
                    return new Vector2(d.x - padding.right, -d.y + padding.bottom);
                case TextAnchor.LowerCenter:
                    return new Vector2(0, -d.y + padding.bottom);
                default:
                    return Vector2.zero;
            }
        }

        private static Vector2 GetZoomOffset(TextAnchor alignment, RectTransform.Axis scrollAxis)
        {
            if (scrollAxis == RectTransform.Axis.Vertical)
            {
                switch (alignment)
                {
                    case TextAnchor.UpperLeft:
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.LowerLeft:
                        return new Vector2(0.5f, 0f);
                    case TextAnchor.UpperRight:
                    case TextAnchor.MiddleRight:
                    case TextAnchor.LowerRight:
                        return new Vector2(-0.5f, 0f);
                    default:
                        return Vector2.zero;
                }
            }
            
            switch (alignment)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.UpperCenter:
                case TextAnchor.UpperRight:
                    return new Vector2(0, -0.5f);
                case TextAnchor.LowerLeft:
                case TextAnchor.LowerCenter:
                case TextAnchor.LowerRight:
                    return new Vector2(0, 0.5f);
                default:
                    return Vector2.zero;
            }
        }
        
        #endregion

        #region Dragging

        protected bool IsDragging { get; private set; }
        private Vector2 _dragCursorStart;
        private float _dragContentStart;
        private float _previousPosition;
        private float _dragVelocity;

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!enabled || eventData.button != PointerEventData.InputButton.Left) return;

            StopScroll();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewport, eventData.position,
                eventData.pressEventCamera, out _dragCursorStart);

            _dragContentStart = NormalizedPosition;
            _previousPosition = _dragContentStart * VirtualSize;
            IsDragging = true;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            IsDragging = false;

            ScrollTo(CurrentIndex);
            _dragVelocity = 0;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!IsDragging || !enabled || eventData.button != PointerEventData.InputButton.Left) return;

            Vector2 cursorPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewport, eventData.position,
                eventData.pressEventCamera, out cursorPosition);

            var offset = _scrollAxis == RectTransform.Axis.Vertical
                ? _dragCursorStart.y - cursorPosition.y
                : _dragCursorStart.x - cursorPosition.x;

            NormalizedPosition = _dragContentStart + offset / VirtualSize;
            Redraw();

            // Track velocity
            var newPosition = NormalizedPosition * VirtualSize;
            _dragVelocity = newPosition - _previousPosition;
            _previousPosition = newPosition;
        }

        #endregion

        #region Animation

        private Coroutine _scrollAnimation;

        private void StopScroll()
        {
            if (_scrollAnimation == null) return;
            StopCoroutine(_scrollAnimation);
            _scrollAnimation = null;
        }

        private IEnumerator AnimateTo(float target, float speed, bool allowLoopAround)
        {
            var start = NormalizedPosition;

            // Loop around?
            if (allowLoopAround)
            {
                if (Mathf.Abs(start - target) > Mathf.Abs(start - (target + 1)))
                    ++target;
                else if (Mathf.Abs(start - target) > Mathf.Abs(start + (1 - target)))
                    --target;
            }

            float p = 0;
            while (p < 1)
            {
                p += Time.unscaledDeltaTime * speed;
                NormalizedPosition = Mathf.LerpUnclamped(start, target, _scrollAnimationCurve.Evaluate(p));
                Redraw();
                yield return null;
            }

            NormalizedPosition = target % 1;
            Redraw();
            
            _scrollAnimation = null;
        }

        #endregion

        #region Inspector

        [SerializeField] [Tooltip("Container for scroller items")]
        private RectTransform _viewport;
        [SerializeField] [Tooltip("Prefab for items in scroller")]
        private TAsset _prefab;

        [Header("Options")] 
        [SerializeField] [Tooltip("Scroll vertical or horizontal")]
        private RectTransform.Axis _scrollAxis;
        [SerializeField] [Tooltip("Alignment of items in the scroller")]
        private TextAnchor _alignment = TextAnchor.MiddleCenter;
        [SerializeField] [Tooltip("Padding around the edge of the view port")]
        private RectOffset _padding = new RectOffset();
        [SerializeField] [Tooltip("Space between the center of each item")]
        private float _spacing = 65;
        [SerializeField] [Tooltip("Continue scrolling at start when reaching the end of the list")]
        private bool _loop = true;
        [SerializeField] [Tooltip("Reverse the order of the items in the scroller")]
        private bool _reverse; 
        
        [Header("Animation")] 
        [SerializeField] [Tooltip("Controls the zoom effect")]
        private AnimationCurve _zoomCurve =
            new AnimationCurve(new Keyframe(0f, 1f, 1f, 1f), new Keyframe(1f, 1.5f, 0f, 0f));
        [SerializeField] [Tooltip("Control the size of the zoom area")]
        private float _zoomArea = 70;
        [SerializeField] [Tooltip("Speed of the ScrollTo and snapping animations")]
        private float _scrollAnimationSpeed = 4;
        [SerializeField] [Tooltip("Acceleration/deceleration for ScrollTo and snapping animation")]
        private AnimationCurve _scrollAnimationCurve =
            new AnimationCurve(new Keyframe(0f, 0f, 2f, 2f), new Keyframe(1f, 1f, 0f, 0f));
        
        [Header("Advanced")]
        [SerializeField] [Tooltip("Disable prefab GameObjects when returning to the pool")]
        private bool _setInactiveInPool = true;
        [SerializeField] [Tooltip("Intertia sensitivity (0 = disabled)")]
        private float _scrollIntertia = 2;
        
        [Space] 
        [SerializeField]
        private ScrollerEvent _onValueChanged = new ScrollerEvent();

        protected virtual void OnValidate()
        {
            // Use this as default view port
            if (_viewport == null) _viewport = GetComponent<RectTransform>();

            // Validate minimum values
            if (_scrollIntertia < 0) _scrollIntertia = 0;
            if (_scrollAnimationSpeed < 0.1f) _scrollAnimationSpeed = 0.1f;
            if (_zoomArea < 1) _zoomArea = 1;
            if (_spacing < 1) _spacing = 1;

            // Redraw changes if playing and initialized
            if (!Application.isPlaying || !IsInitialized) return;
            HardRedraw(false);
        }

        private void OnDrawGizmosSelected()
        {
            // Refresh cached values for accuracy in gizmos
            RefreshCachedValues();
            
            // Draw zoom area
            Gizmos.color = new Color(1f, 1f, 1f, 0.4f);
            var center = _viewport.position + new Vector3(_itemOffset.x, _itemOffset.y, 0);
            Gizmos.DrawWireCube(center, new Vector3(_zoomArea, _zoomArea, 0) * 2);
        }
        
        #endregion
    }
    
    [Serializable]
    public class ScrollerEvent : UnityEvent<int>
    {
    }
}