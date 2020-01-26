using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil {

    public class ToggleGoVisibility : MonoBehaviour, IPointerClickHandler {

        public Transform target;

        public void OnPointerClick(PointerEventData eventData) { ToggleVisibilityOfTarget(); }

        public void ToggleVisibilityOfTarget() {
            if (target == null) { throw Log.e("Toggle-target not set", gameObject); }
            target.gameObject.SetActiveV2(!target.gameObject.activeSelf);
        }

    }

}