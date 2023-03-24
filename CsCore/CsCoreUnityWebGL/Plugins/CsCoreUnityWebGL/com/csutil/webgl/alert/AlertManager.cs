using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

namespace com.csutil.webgl {

    /// <summary> This is a script that manages the connection from unity to the alert functions 
    /// of a browser, when the project is being compiled to WebGL </summary>
    public class AlertManager : MonoBehaviour {

        /// <summary> Import JSLib functions:
        /// 
        /// This is where we reference the javaScript funtctions we have written
        /// into our .jslib file. If you have included cscore as a module, the
        /// javaScript code will automatically be served to the browser.
        ///
        /// More info at:
        /// https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html
        /// </summary>
        #region jsFunctionImports

        [DllImport("__Internal")]
        private static extern void createOnUnloadHandlerjs();

        [DllImport("__Internal")]
        private static extern void triggerBrowserAlertjs(string message);

        [DllImport("__Internal")]
        private static extern void deactivateOnQuitPromptjs();

        [DllImport("__Internal")]
        private static extern void activateOnQuitPromptjs();

        #endregion

        /// <summary> Fires when the closes the browser window </summary>
        public OnBrowserCloseEvent onBrowserClose = new OnBrowserCloseEvent();

        private bool _showUnsavedChangesWarningOnPageClose = false;
        public bool ShowUnsavedChangesWarningOnPageClose {
            set {
                if (value) { activateOnQuitPromptjs(); } else { deactivateOnQuitPromptjs(); }
                _showUnsavedChangesWarningOnPageClose = value;
            }
            get => _showUnsavedChangesWarningOnPageClose;
        }

        void Start() {
            IoC.inject.SetSingleton(this);
            if (!EnvironmentV2.isEditor) {
                createOnUnloadHandlerjs();
            }
        }

        /// <summary> Send alert message to JSLib File </summary>
        public void ShowBrowserAlertMessage(string alertMessage) {
            triggerBrowserAlertjs(alertMessage);
        }

        /// <summary> This function is triggered if the user tries to close the browser window </summary>
        void onTabCloseAttempt() {
            onBrowserClose?.Invoke();
        }

        [System.Serializable]
        public class OnBrowserCloseEvent : UnityEvent {
        }

    }

}