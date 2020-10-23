using UnityEngine;
using UnityEngine.Events;

namespace com.csutil {

    public class OnDestroyListener : MonoBehaviour {

        [System.Serializable]
        public class OnDestroyEvent : UnityEvent { }

        public OnDestroyEvent onDestroy = new OnDestroyEvent();

        private void OnDestroy() {
            if (onDestroy.IsNullOrEmpty()) { throw Log.e("OnDestroyListener had no listeners"); }
            onDestroy.Invoke();
        }

    }

}