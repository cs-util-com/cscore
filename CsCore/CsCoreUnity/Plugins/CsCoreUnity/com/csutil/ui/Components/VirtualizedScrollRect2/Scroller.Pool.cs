//  
// Copyright (c) 2019 Shane Harper
// Licensed under the MIT. See LICENSE file full license information.  
//  

using System.Collections.Generic;
using UnityEngine;

namespace Sharper.Scroller
{
    public abstract partial class Scroller<TAsset, TData>
    {
        private class Pool : Pool<TAsset>
        {
            private readonly RectTransform _parent;

            public Pool(TAsset original, RectTransform parent, int capacity = 0) : base(original, capacity)
            {
                _parent = parent;
            }

            protected override TAsset CreateNewInstance()
            {
                return Instantiate(Original, _parent);
            }

            public override void Return(TAsset item)
            {
                base.Return(item);
                item.RectTransform.localScale = Vector3.zero;
                item.OnReturnToPool();
            }
        }
    }

    /// <summary>
    ///     A strongly typed pool of objects. When the pool is empty, new instances will be created when requested
    /// </summary>
    /// <typeparam name="T">The type of elements in the pool</typeparam>
    public abstract class Pool<T>
    {
        protected readonly T Original;

        /// <summary>
        ///     Create new pool
        /// </summary>
        /// <param name="original">Original asset</param>
        /// <param name="capacity">The initial number of elements that the pool can contain.</param>
        protected Pool(T original, int capacity = 0)
        {
            Original = original;
            _queue = new Queue<T>(capacity);
        }

        /// <summary>
        ///     Create a new item instance
        /// </summary>
        /// <remarks>Called when no items are available in the queue</remarks>
        protected abstract T CreateNewInstance();

        /// <summary>
        ///     Get an item from the pool or create a new one if none is available
        /// </summary>
        /// <returns>Returns a copy of the original asset</returns>
        public virtual T Get()
        {
            return _queue.Count > 0 ? _queue.Dequeue() : CreateNewInstance();
        }

        /// <summary>
        ///     Get an item from the pool or create a new one if none is available
        /// </summary>
        /// <param name="isNew">Returns true if the returned asset has just been created</param>
        /// <returns>Returns a copy of the original asset</returns>
        public virtual T Get(out bool isNew)
        {
            if (_queue.Count > 0)
            {
                isNew = false;
                return _queue.Dequeue();
            }

            isNew = true;
            return CreateNewInstance();
        }

        /// <summary>
        ///     Return item to the pool
        /// </summary>
        /// <param name="item">Item to be returned to the pool</param>
        public virtual void Return(T item)
        {
            _queue.Enqueue(item);
        }

        #region Queue

        /// <summary>
        ///     Number of items currently queued in the pool
        /// </summary>
        public int Count
        {
            get { return _queue.Count; }
        }

        private readonly Queue<T> _queue;

        #endregion
    }
}