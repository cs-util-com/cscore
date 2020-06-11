using System;
using UnityEngine;

namespace com.csutil.ui {

    public class ThemeColor : MonoBehaviour {

        public enum ColorNames {
            custom,
            accent, accentContrast, accentContrastWeak,
            primary, primaryContrast, primaryContrastWeak,
            background, backgroundContrast, backgroundContractWeak,
            transparent, transparentContrast, transparentContrastWeak,
            shadow, shadowContrast, shadowContrastWeak,
            warning, warningContrast, warningContrastWeak,
            element, elementContrast, elementContrastWeak,
            button, buttonContrast, buttonContrastWeak
        }

        [ShowPropertyInInspector]
        public ColorNames colorNameSuggestion {
            get { return ColorNames.custom.TryParse(colorName); }
            set { if (ColorNames.custom != value) { colorName = value.GetEntryName(); } }
        }

        [SerializeField]
        private string _colorName;
        public string colorName { get { return _colorName; } set { _colorName = value; Refresh(); } }

        private void Refresh() { IoC.inject.GetOrAddComponentSingleton<Theme>(this).ApplyTheme(colorName, this); }

    }

}
