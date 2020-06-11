using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace com.csutil.ui {

    public class ToggleGoVisibility : MonoBehaviour, IPointerClickHandler {

        public Transform target;
        public OnToggleEvent onToggled;

        public void OnPointerClick(PointerEventData eventData) { ToggleVisibilityOfTarget(); }

        public void ToggleVisibilityOfTarget() {
            if (target == null) { throw Log.e("Toggle-target not set", gameObject); }
            target.gameObject.SetActiveV2(!target.gameObject.activeSelf);
            onToggled?.Invoke(target.gameObject.activeSelf);
        }

        private void OnValidate() {
            if (target == null) { // Try to auto assign in Editor if there is a single hidden direct child:
                try { target = gameObject.GetChildrenIEnumerable().Single(x => !x.activeSelf).transform; } catch { }
            }
        }

        [System.Serializable]
        public class OnToggleEvent : UnityEvent<bool> { }

    }

}