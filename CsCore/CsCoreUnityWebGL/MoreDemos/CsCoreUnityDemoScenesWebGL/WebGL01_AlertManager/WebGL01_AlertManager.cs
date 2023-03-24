using UnityEngine;
using UnityEngine.UI;
using com.csutil.webgl;

namespace com.csutil.tests.ui {

    /// <summary> Gets the textField input and sends it with the AlertManager </summary>
    public class WebGL01_AlertManager : MonoBehaviour {

        private void OnEnable() {

            var links = gameObject.GetLinkMap();

            links.Get<Button>("ActivateWarning").SetOnClickAction(delegate {
                GetAlertManager().ShowUnsavedChangesWarningOnPageClose = true;
            });
            links.Get<Button>("DeactivateWarning").SetOnClickAction(delegate {
                GetAlertManager().ShowUnsavedChangesWarningOnPageClose = false;
            });
            links.Get<InputField>("AlertTextInput").SetOnValueChangedActionThrottled(newText => {
                Log.MethodEnteredWith(newText);
                GetAlertManager().ShowBrowserAlertMessage(newText);
            }, 2000); // after 2 seconds delay show the entered text

        }

        private AlertManager GetAlertManager() { return IoC.inject.GetOrAddSingleton<AlertManager>(this); }

    }

}