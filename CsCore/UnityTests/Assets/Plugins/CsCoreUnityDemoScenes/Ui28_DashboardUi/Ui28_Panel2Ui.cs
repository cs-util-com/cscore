using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class Ui28_Panel2Ui : MonoBehaviour {

        private void OnEnable() {
            var ui = gameObject.GetLinkMap();
            ui.Get<Button>("Panel2_Button1").SetOnClickAction(delegate {
                Toast.Show("Button 2 was pressed");
            });
            ui.Get<InputField>("Panel2_InputField1").SetOnValueChangedActionThrottled(input => {
                Toast.Show("Input text was: " + input);
            }, delayInMs: 2000);
            ui.Get<Dropdown>("Panel2_DropDown1").SetOnValueChangedAction(selection => {
                Toast.Show("New dropdown selection was:" + selection);
                return true;
            });
        }

    }

}