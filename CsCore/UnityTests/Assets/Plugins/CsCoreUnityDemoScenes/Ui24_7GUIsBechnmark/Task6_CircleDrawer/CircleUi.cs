using com.csutil.ui;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil.tests {

    internal class CircleUi : MonoBehaviour, IPointerClickHandler {

        public string circleId;
        public Action<string, bool> OnCircleClicked;

        private void OnValidate() { // When circle is added through Unity editor, init its anchors and local position:
            if (GetComponent<RectTransform>().SetAnchorsBottomLeft()) { GetComponent<RectTransform>().localPosition = Vector2.zero; }
        }

        public void OnPointerClick(PointerEventData eventData) { OnCircleClicked(circleId, eventData.IsRightClick()); }

        public void VisualizeCircleAsSelected(bool isSelected) {
            GetComponent<ThemeColor>().ApplyColor(isSelected ? Color.gray : Color.white);
        }

    }

}