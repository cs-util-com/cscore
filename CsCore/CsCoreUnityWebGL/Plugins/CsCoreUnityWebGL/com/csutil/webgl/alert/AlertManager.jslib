/**
 * This is how javascript functions are implemented in a way that
 * makes them callable from within the C# code. More info at:
 * https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html
 */
mergeInto(LibraryManager.library, {

    /**
     * This function is executed to add a listener to the onClose 
     * Event that is provided by the browser
     */
    createOnUnloadHandlerjs: function () {
        // Initialized this variable so it's not ambiguous
        this._OnUnloadManager_PreventClosing = false;

        // The event we want to attach to is called "beforeunload"
        window.addEventListener("beforeunload", (event) => {

            //Send a message back to the unitySide to inform the 
            //Programm that the user attempts to close the tab
            SendMessage("AlertManager", "onTabCloseAttempt");

            if (!this._OnUnloadManager_PreventClosing) {
                return null;
            }

            //This causes the browser to display a warning 
            event.returnValue = 1;
        });
    },

    /**
     * After this has been called there will a warning
     */
    activateOnQuitPromptjs: function () {
        this._OnUnloadManager_PreventClosing = true;
    },

    /**
     * After this has been called there will be no warning
     */
    deactivateOnQuitPromptjs: function () {
        this._OnUnloadManager_PreventClosing = false;
    },

    /**
     * This function creates a browser warning message
     * @param messagePtr Pointer to the string location from the C# part
     */
    triggerBrowserAlertjs: function (messagePtr) {
        alert(UTF8ToString(messagePtr));
    }
});