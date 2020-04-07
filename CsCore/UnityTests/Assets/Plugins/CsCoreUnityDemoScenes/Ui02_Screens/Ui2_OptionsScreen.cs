using com.csutil.ui;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class Ui2_OptionsScreen : MonoBehaviour {

        private const string TOGGLE1 = "Toggle (1)";
        private const string TOGGLE2 = "Toggle (2)";
        private const string TOGGLE3 = "Toggle (3)";

        private Dictionary<string, Link> links;

        void Start() {
            links = gameObject.GetLinkMap();
            toggle1().isOn = PlayerPrefsV2.GetBool(TOGGLE1, false);
            toggle2().isOn = PlayerPrefsV2.GetBool(TOGGLE2, false);
            toggle3().isOn = PlayerPrefsV2.GetBool(TOGGLE3, false);
            links.Get<Button>("ConfirmButton").SetOnClickAction(delegate {
                PlayerPrefsV2.SetBool(TOGGLE1, toggle1().isOn);
                PlayerPrefsV2.SetBool(TOGGLE2, toggle2().isOn);
                PlayerPrefsV2.SetBool(TOGGLE3, toggle3().isOn);
                gameObject.GetViewStack().SwitchBackToLastView(gameObject);
            });
        }

        private Toggle toggle1() { return links.Get<Toggle>(TOGGLE1); }
        private Toggle toggle2() { return links.Get<Toggle>(TOGGLE2); }
        private Toggle toggle3() { return links.Get<Toggle>(TOGGLE3); }

    }

}