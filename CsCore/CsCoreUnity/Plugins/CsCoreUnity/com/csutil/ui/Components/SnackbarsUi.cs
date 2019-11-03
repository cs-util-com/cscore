using com.csutil.ui;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public static class Snackbar {

        public const int DEFAULT_DURATION_IN_MS = 5000;

        public static GameObject Show(string snackbarMsg, int displayDurationInMs = DEFAULT_DURATION_IN_MS) {
            return IoC.inject.GetOrAddSingleton<SnackbarsUi>(null, InitSnackbarsUi).Show(snackbarMsg, null, displayDurationInMs);
        }

        public static GameObject Show(string msg, Action<GameObject> onClick, int displayDurationInMs = DEFAULT_DURATION_IN_MS) {
            return IoC.inject.GetOrAddSingleton<SnackbarsUi>(null, InitSnackbarsUi).Show(msg, onClick, displayDurationInMs);
        }

        private static SnackbarsUi InitSnackbarsUi() {
            var targetCanvas = CanvasFinder.GetOrAddRootCanvas().gameObject;
            var snackbarContainer = targetCanvas.AddChild(ResourcesV2.LoadPrefab("Messages/SnackbarContainer1"));
            return snackbarContainer.GetOrAddComponent<SnackbarsUi>();
        }

    }

    public class SnackbarsUi : MonoBehaviour {

        private GameObject snackbarsContainer;
        private void OnEnable() { snackbarsContainer = gameObject.GetLinkMap().Get<GameObject>("MessageContainer"); }

        public GameObject Show(string snackbarMsg, Action<GameObject> snackbarAction, int displayDurationInMs) {
            var newSnackbar = ResourcesV2.LoadPrefab("Messages/Snackbar");
            var snackbarUiElems = newSnackbar.GetLinkMap();
            snackbarUiElems.Get<Text>("Message").text = snackbarMsg;
            var btn = snackbarUiElems.Get<Button>("SnackbarButton");
            if (snackbarAction != null) { btn.SetOnClickAction(snackbarAction); } else { btn.gameObject.Destroy(); }
            newSnackbar.GetComponent<MonoBehaviour>().ExecuteDelayed(() => newSnackbar.Destroy(), displayDurationInMs);
            snackbarsContainer.AddChild(newSnackbar);
            return newSnackbar;
        }

    }

}