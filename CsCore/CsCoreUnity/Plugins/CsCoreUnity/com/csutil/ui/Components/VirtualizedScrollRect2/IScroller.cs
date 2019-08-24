//  
// Copyright (c) 2019 Shane Harper
// Licensed under the MIT. See LICENSE file full license information.  
//  

using System.Collections.Generic;
using UnityEngine;

namespace Sharper.Scroller
{
    public interface IScroller<TData> : IScroller
        where TData : IScrollerData
    {
        /// <summary>
        ///     The currently centered item
        /// </summary>
        TData CurrentItem { get; }
        
        /// <summary>
        ///     Initialize scroller with items (You may call this again to update the scroller content)
        /// </summary>
        /// <param name="data">Array or List containing the data objects to be displayed</param>
        void SetItems(IList<TData> data);
    }

    public interface IScroller
    {
        /// <summary>
        ///     Returns true if the scroller has been initialized
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        ///     Event triggered when the selected value changes
        /// </summary>
        ScrollerEvent OnValueChanged { get; }
        
        /// <summary>
        ///     The virtual size of the scroll area
        /// </summary>
        float VirtualSize { get; }
        
        /// <summary>
        ///     Number of items loaded into the scroller
        /// </summary>
        int ItemCount { get; }
        
        /// <summary>
        ///     Index of the currently centered item
        /// </summary>
        int CurrentIndex { get; }
        
        /// <summary>
        ///     Jump to specific index (no animation)
        /// </summary>
        void JumpTo(int index);
        
        /// <summary>
        ///     Animate smoothly to index
        /// </summary>
        void ScrollTo(int index);
    }
    
    /// <summary>
    ///     Data object used to populate a Scroller
    /// </summary>
    public interface IScrollerData
    {
    }
    
    /// <summary>
    ///     Prefab used to display an item in a Scroller
    /// </summary>
    public interface IScrollerPrefab<TData> where TData : IScrollerData
    {
        /// <summary>
        ///     The root transform. Used to move the prefab and to get the it's size
        /// </summary>
        RectTransform RectTransform { get; }
        
        /// <summary>
        ///     Set the index of this prefab and the data to be used with it
        /// </summary>
        void SetData(int index, TData data);
        
        /// <summary>
        ///     Called when returning this prefab to the pool
        /// </summary>
        void OnReturnToPool();
    }
}