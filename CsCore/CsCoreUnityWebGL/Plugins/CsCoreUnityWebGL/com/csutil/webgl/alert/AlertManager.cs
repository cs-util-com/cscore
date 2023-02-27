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
        private static extern void triggerAlert(string message);

        [DllImport("__Internal")]
        private static extern void deactivateOnSavedWarning();

        [DllImport("__Internal")]
        private static extern void activateOnSavedWarning();
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
        public void sendAlert(string message) {
            triggerAlert(message);
        }

        /// <summary>
        /// This function is triggered if the user tries to close the browser window
        /// </summary>
        void onClose() {
            Debug.Log("The user attempted to close the tab");
        }

        /// <summary>
        /// Deactivates the unsaved changes warning 
        /// </summary>
        public void deactivate() {
            Debug.Log("Deactivate from Unity");
            deactivateOnSavedWarning();
        }

        /// <summary>
        /// Activates the unsaved changes warning 
        /// </summary>
        public void activate() {
            Debug.Log("Activate from Unity");
            activateOnSavedWarning();
        }
    }

}