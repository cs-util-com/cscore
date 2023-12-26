using com.csutil.ui;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil {

    public static class Toast {

        public const int DEFAULT_DURATION_IN_MS = 3000;

        public static GameObject Show(string toastMsg, int displayDurationInMs = DEFAULT_DURATION_IN_MS) {
            return IoC.inject.GetOrAddSingleton<ToastsUi>(null, InitToastsUi).Show(null, toastMsg, displayDurationInMs);
        }

        public static GameObject Show(string toastCaption, string toastMsg, int displayDurationInMs = DEFAULT_DURATION_IN_MS) {
            return IoC.inject.GetOrAddSingleton<ToastsUi>(null, InitToastsUi).Show(toastCaption, toastMsg, displayDurationInMs);
        }

        private static ToastsUi InitToastsUi() {
            if (!Application.isPlaying) { throw Log.e("Showing toasts not supported in edit mode"); }
            var targetCanvas = RootCanvas.GetOrAddRootCanvasV2().gameObject;
            var toastContainer = targetCanvas.AddChild(ResourcesV2.LoadPrefab("Messages/ToastContainer1"));
            return toastContainer.GetOrAddComponent<ToastsUi>();
        }

    }

    public class ToastsUi : MonoBehaviour, IDisposableV2 {

        private GameObject toastsContainer;

        public DisposeState IsDisposed => DisposeStateHelper.FromBool(this.IsDestroyed());

        private void OnEnable() { FindToastContainerIfNeeded(); }

        private void FindToastContainerIfNeeded() {
            if (toastsContainer == null) {
                toastsContainer = gameObject.GetLinkMap().Get<GameObject>("MessageContainer");
            }
        }

        public GameObject Show(string toastCaption, string toastMessage, int displayDurationInMs) {
            var newToast = ResourcesV2.LoadPrefab("Messages/Toast");
            var toastUiElems = newToast.GetLinkMap();
            InitText(toastUiElems, "Caption", toastCaption);
            InitText(toastUiElems, "Message", toastMessage);
            newToast.GetComponentV2<MonoBehaviour>().ExecuteDelayed(() => newToast.Destroy(), displayDurationInMs);
            FindToastContainerIfNeeded();
            toastsContainer.AddChild(newToast);
            return newToast;
        }

        private static void InitText(Dictionary<string, Link> map, string id, string text) {
            if (text.IsNullOrEmpty()) { map.Get<GameObject>(id).SetActiveV2(false); } else { map.Get<Text>(id).text = text; }
        }

        public void Dispose() { this.gameObject.Destroy(); }
    }

}