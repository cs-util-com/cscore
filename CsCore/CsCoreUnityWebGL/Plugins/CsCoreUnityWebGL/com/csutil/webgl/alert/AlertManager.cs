using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// This is a script that manages the connection from unity to the alert functions 
/// of a browser, when the project is being compiled to WebGL
/// </summary>
///
namespace com.csutil.webgl.alert {

    public class AlertManager : MonoBehaviour {

        /// <summary>
        /// Import JSLib functions
        /// </summary>
        #region jsFunctionImports
        [DllImport("__Internal")]
        private static extern void createOnUnload();

        [DllImport("__Internal")]
        private static extern void triggerBrowserAlertjs(string message);

        [DllImport("__Internal")]
        private static extern void deactivateOnQuitPromptjs();

        [DllImport("__Internal")]
        private static extern void activateOnQuitPromptjs();
        #endregion


        void Start() {
            createOnUnload();
        }
        /// <summary>
        /// Send alert message to JSLib File
        /// </summary>
        /// <param name="message">
        /// Alert message
        /// </param>
        public void triggerBrowserAlert(string message) {
            triggerBrowserAlertjs(message);
        }

        /// <summary>
        /// This function is triggered if the user tries to close the browser window
        /// </summary>
        void onTabCloseAttempt() {
            Debug.Log("The user attempted to close the tab");
        }

        /// <summary>
        /// Deactivates the unsaved changes warning 
        /// </summary>
        public void deactivateOnQuitPrompt() {
            Debug.Log("Deactivate from Unity");
            deactivateOnQuitPromptjs();
        }

        /// <summary>
        /// Activates the unsaved changes warning 
        /// </summary>
        public void activateOnQuitPrompt() {
            Debug.Log("Activate from Unity");
            activateOnQuitPromptjs();
        }
    }

}