using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil.tests {

    class CircleCanvas : MonoBehaviour, IPointerClickHandler {

        public Action<Task6_CircleDrawer.MyCircle> OnCicleCreated;

        public void OnPointerClick(PointerEventData clickEvent) {
            Log.MethodEnteredWith(clickEvent.button);
            if (clickEvent.IsLeftClick()) {
                var pos = clickEvent.localPosition(ignoreGlobalScale: true);
                OnCicleCreated.Invoke(new Task6_CircleDrawer.MyCircle(Guid.NewGuid().ToString(), pos.x, pos.y, diameter: 1));
            }
        }

    }

}