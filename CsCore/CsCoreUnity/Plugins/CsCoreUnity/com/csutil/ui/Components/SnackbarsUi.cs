using com.csutil.ui;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil {

    public static class Snackbar {

        public const int DEFAULT_DURATION_IN_MS = 5000;

        public static GameObject Show(string snackbarMsg, int displayDurationInMs = DEFAULT_DURATION_IN_MS) {
            return IoC.inject.GetOrAddSingleton<SnackbarsUi>(null, InitSnackbarsUi).Show(snackbarMsg, null, null, displayDurationInMs);
        }

        public static GameObject Show(string msg, string buttonMsg, Action<GameObject> onClick, int displayDurationInMs = DEFAULT_DURATION_IN_MS) {
            return IoC.inject.GetOrAddSingleton<SnackbarsUi>(null, InitSnackbarsUi).Show(msg, buttonMsg, onClick, displayDurationInMs);
        }

        private static SnackbarsUi InitSnackbarsUi() {
            var targetCanvas = RootCanvas.GetOrAddRootCanvas().gameObject;
            var snackbarContainer = targetCanvas.AddChild(ResourcesV2.LoadPrefab("Messages/SnackbarContainer1"));
            return snackbarContainer.GetOrAddComponent<SnackbarsUi>();
        }

    }

    public class SnackbarsUi : MonoBehaviour {

        private GameObject snackbarsContainer;
        private void OnEnable() { snackbarsContainer = gameObject.GetLinkMap().Get<GameObject>("MessageContainer"); }

        public GameObject Show(string snackbarMsg, string buttonMsg, Action<GameObject> snackbarAction, int displayDurationInMs) {
            var newSnackbar = ResourcesV2.LoadPrefab("Messages/Snackbar");
            var map = newSnackbar.GetLinkMap();
            map.Get<Text>("Message").text = snackbarMsg;
            if (snackbarAction != null && !buttonMsg.IsNullOrEmpty()) {
                map.Get<Text>("SnackbarButton").text = buttonMsg;
                map.Get<Button>("SnackbarButton").SetOnClickAction(snackbarAction);
            } else {
                map.Get<GameObject>("SnackbarButton").Destroy();
            }
            newSnackbar.GetComponentV2<MonoBehaviour>().ExecuteDelayed(() => newSnackbar.Destroy(), displayDurationInMs);
            snackbarsContainer.AddChild(newSnackbar);
            return newSnackbar;
        }

    }

}