using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    [RequireComponent(typeof(Toggle))]
    public class RadioButton : MonoBehaviour {

        private void OnEnable() {
            var group = GetToggleGroup();
            GetComponent<Toggle>().group = group;
        }

        public ToggleGroup GetToggleGroup() {
            var group = gameObject.GetComponentInParents<ToggleGroup>();
            if (group == null) { throw new MissingFieldException("toggleGroup not found in any parents, radio button will not work"); }
            return group;
        }

    }

}