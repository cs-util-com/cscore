using System;
using UnityEngine.Events;

namespace com.csutil {

    public interface IUnityEventV2 {
        int GetAllEventCount();
    }
    
    public static class UnityEventV2Extensions {

        /// <summary> Adds a child GameObject to the calling new parent GameObject </summary>
        public static bool IsNullOrEmptyV2(this IUnityEventV2 self) {
            if (self == null) { return true; }
            return self.GetAllEventCount() == 0;
        }
        
        [Obsolete("Use UnityEventV2.IsNullOrEmptyV2() instead", true)]
        public static bool IsNullOrEmpty(this UnityEventBase self) {
            throw new NotImplementedException();
        }

    }
    
    public class UnityEventV2 : UnityEvent, IUnityEventV2 {
        
        private int _runtimeEventCount;

        public static UnityEventV2 operator +(UnityEventV2 a, UnityAction b) {
            a.AddListener(b);
            a._runtimeEventCount++;
            return a;
        }

        public static UnityEventV2 operator -(UnityEventV2 a, UnityAction b) {
            a.RemoveListener(b);
            a._runtimeEventCount--;
            return a;
        }
        
        public void AddListenerV2(UnityAction b) {
            base.AddListener(b);
            _runtimeEventCount++;
        }
        
        public void RemoveListenerV2(UnityAction b) {
            base.RemoveListener(b);
            _runtimeEventCount--;
        }
        
        public void RemoveAllListenersV2() {
            base.RemoveAllListeners();
            _runtimeEventCount = 0;
        }

        public int GetAllEventCount() {
            return this.GetPersistentEventCount() + _runtimeEventCount;
        }
        
    }

    public class UnityEventV2<T0> : UnityEvent<T0>, IUnityEventV2 {
        
        private int _runtimeEventCount;

        public static UnityEventV2<T0> operator +(UnityEventV2<T0> a, UnityAction<T0> b) {
            a.AddListener(b);
            a._runtimeEventCount++;
            return a;
        }

        public static UnityEventV2<T0> operator -(UnityEventV2<T0> a, UnityAction<T0> b) {
            a.RemoveListener(b);
            a._runtimeEventCount--;
            return a;
        }
        
        public void AddListenerV2(UnityAction<T0> b) {
            base.AddListener(b);
            _runtimeEventCount++;
        }
        
        public void RemoveListenerV2(UnityAction<T0> b) {
            base.RemoveListener(b);
            _runtimeEventCount--;
        }
        
        public void RemoveAllListenersV2() {
            base.RemoveAllListeners();
            _runtimeEventCount = 0;
        }

        public int GetAllEventCount() { return this.GetPersistentEventCount() + _runtimeEventCount; }
        
    }
    
    public class UnityEventV2<T0, T1> : UnityEvent<T0, T1>, IUnityEventV2 {
        
        private int _runtimeEventCount;

        public static UnityEventV2<T0, T1> operator +(UnityEventV2<T0, T1> a, UnityAction<T0, T1> b) {
            a.AddListener(b);
            a._runtimeEventCount++;
            return a;
        }

        public static UnityEventV2<T0, T1> operator -(UnityEventV2<T0, T1> a, UnityAction<T0, T1> b) {
            a.RemoveListener(b);
            a._runtimeEventCount--;
            return a;
        }

        public void AddListenerV2(UnityAction<T0, T1> b) {
            base.AddListener(b);
            _runtimeEventCount++;
        }
        
        public void RemoveListenerV2(UnityAction<T0, T1> b) {
            base.RemoveListener(b);
            _runtimeEventCount--;
        }
        
        public void RemoveAllListenersV2() {
            base.RemoveAllListeners();
            _runtimeEventCount = 0;
        }
        
        public int GetAllEventCount() { return this.GetPersistentEventCount() + _runtimeEventCount; }
        
    }
    
    public class UnityEventV2<T0, T1, T2> : UnityEvent<T0, T1, T2>, IUnityEventV2 {
        
        private int _runtimeEventCount;

        public static UnityEventV2<T0, T1, T2> operator +(UnityEventV2<T0, T1, T2> a, UnityAction<T0, T1, T2> b) {
            a.AddListener(b);
            a._runtimeEventCount++;
            return a;
        }

        public static UnityEventV2<T0, T1, T2> operator -(UnityEventV2<T0, T1, T2> a, UnityAction<T0, T1, T2> b) {
            a.RemoveListener(b);
            a._runtimeEventCount--;
            return a;
        }
        
        public void AddListenerV2(UnityAction<T0, T1, T2> b) {
            base.AddListener(b);
            _runtimeEventCount++;
        }
        
        public void RemoveListenerV2(UnityAction<T0, T1, T2> b) {
            base.RemoveListener(b);
            _runtimeEventCount--;
        }
        
        public void RemoveAllListenersV2() {
            base.RemoveAllListeners();
            _runtimeEventCount = 0;
        }

        public int GetAllEventCount() { return this.GetPersistentEventCount() + _runtimeEventCount; }
        
    }
    
}