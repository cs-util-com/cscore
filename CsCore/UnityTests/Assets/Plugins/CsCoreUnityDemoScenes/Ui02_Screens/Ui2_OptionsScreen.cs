using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class Ui2_OptionsScreen : MonoBehaviour {

        private const string TOGGLE1 = "Toggle (1)";
        private const string TOGGLE2 = "Toggle (2)";
        private const string TOGGLE3 = "Toggle (3)";

        private Dictionary<string, Link> links;

        void Start() { OnLoad().LogOnError(); }

        private async Task OnLoad() {
            links = gameObject.GetLinkMap();
            toggle1().isOn = await Preferences.instance.Get(TOGGLE1, false);
            toggle2().isOn = await Preferences.instance.Get(TOGGLE2, false);
            toggle3().isOn = await Preferences.instance.Get(TOGGLE3, false);
            await links.Get<Button>("ConfirmButton").SetOnClickAction(async delegate {
                await Preferences.instance.Set(TOGGLE1, toggle1().isOn);
                await Preferences.instance.Set(TOGGLE2, toggle2().isOn);
                await Preferences.instance.Set(TOGGLE3, toggle3().isOn);
                gameObject.GetViewStack().SwitchBackToLastView(gameObject);
            });
        }

        private Toggle toggle1() { return links.Get<Toggle>(TOGGLE1); }
        private Toggle toggle2() { return links.Get<Toggle>(TOGGLE2); }
        private Toggle toggle3() { return links.Get<Toggle>(TOGGLE3); }

    }

}