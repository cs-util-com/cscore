using System.Runtime.InteropServices;
using UnityEngine;

namespace com.csutil.webgl.alert {

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

        void Start() {
            createOnUnloadHandlerjs();
        }

        /// <summary> Send alert message to JSLib File </summary>
        public void triggerBrowserAlert(string alertMessage) {
            triggerBrowserAlertjs(alertMessage);
        }

        /// <summary> This function is triggered if the user tries to close the browser window </summary>
        void onTabCloseAttempt() {
            Log.e("The user attempted to close the tab");
            // TODO forward to a function that can be set from the unity editor
        }

        /// <summary> Deactivates the unsaved changes warning </summary>
        public void deactivateOnQuitPrompt() {
            Debug.Log("Deactivate from Unity");
            deactivateOnQuitPromptjs();
        }

        /// <summary> Show an "unsaved changes" warning </summary>
        public void activateOnQuitPrompt() {
            Log.d("Show an 'unsaved changes' warning");
            activateOnQuitPromptjs();
        }

    }

}