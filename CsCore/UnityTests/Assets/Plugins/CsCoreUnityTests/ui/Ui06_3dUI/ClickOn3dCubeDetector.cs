using com.csutil;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickOn3dCubeDetector : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventData) {
        Log.d("OnPointerClick on " + gameObject, gameObject);
    }

}