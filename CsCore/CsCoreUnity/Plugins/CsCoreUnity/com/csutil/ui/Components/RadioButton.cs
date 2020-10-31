using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    [RequireComponent(typeof(Toggle))]
    class RadioButton : MonoBehaviour {

        private void OnEnable() {
            var group = gameObject.GetComponentInParents<ToggleGroup>();
            if (group == null) { throw Log.e("toggleGroup not found in any parents, radio button will not work"); }
            GetComponent<Toggle>().group = group;
        }

    }

}