using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.csutil.webgl.alert;


namespace com.csutil.tests.ui {
    /// <summary>
    /// Gets the textField input and sends it with the AlertManager
    /// </summary>
    public class TriggerAlert : MonoBehaviour {
        public GameObject textField;
        public GameObject alertManager;


        public void alertBrowserFromTextField() {
            alertManager.GetComponent<AlertManager>().sendAlert(textField.GetComponent<InputField>().text);
        }


    }
}
