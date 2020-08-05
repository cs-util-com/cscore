using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.csutil.ui.Components {

    /// <summary> A non drawing graphic that can block raycasts </summary>
    public class RaycastBlockerGraphic : Graphic, IPointerClickHandler {

        public override void SetMaterialDirty() { }
        public override void SetVerticesDirty() { }

        // Do nothing when clicked
        public void OnPointerClick(PointerEventData eventData) { }

    }

}
