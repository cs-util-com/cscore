using com.csutil.ui;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil.tests {

    internal class CircleUi : MonoBehaviour, IPointerClickHandler {

        public string circleId;
        public Action<string, bool> OnCircleClicked;

        private void OnValidate() { // When circle is added through Unity editor, init its anchors and local position:
            var rectTransform = gameObject.GetComponentV2<RectTransform>();
            if (rectTransform.SetAnchorsBottomLeft()) { rectTransform.localPosition = Vector2.zero; }
        }

        public void OnPointerClick(PointerEventData eventData) { OnCircleClicked(circleId, eventData.IsRightClick()); }

        public void VisualizeCircleAsSelected(bool isSelected) {
            gameObject.GetComponentV2<ThemeColor>().ApplyColor(isSelected ? Color.gray : Color.white);
        }

    }

}