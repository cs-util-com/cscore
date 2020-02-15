using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil.tests.ui {

    public class ClickOn3dCubeDetector : MonoBehaviour, IPointerClickHandler {

        public void OnPointerClick(PointerEventData eventData) {
            Log.d("OnPointerClick on " + gameObject, gameObject);
        }

    }

}