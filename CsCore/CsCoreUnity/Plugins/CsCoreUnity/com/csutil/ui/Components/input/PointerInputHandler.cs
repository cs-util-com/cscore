using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace com.csutil.ui {

    /// <summary> Can detect inputs like click, double click and long press </summary>
    public class PointerInputHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {

        public UnityEvent onClick = new UnityEvent();
        public UnityEvent onDoubleClick = new UnityEvent();
        public UnityEvent onLongPressStart = new UnityEvent();
        public UnityEvent onLongPressEnd = new UnityEvent();

        public int longPressDurInMs = 500;
        public int doubleClickTimeoutInMs = 300;
        public float maxPixelDistance = 10;

        enum ClickResult { up, click, longPress }
        TaskCompletionSource<ClickResult> pointerUpTask = new TaskCompletionSource<ClickResult>();

        public PointerEventData LastPointerDown { get; private set; }
        public PointerEventData LatestPointer { get; private set; }
        public PointerEventData LastPointerUp { get; private set; }

        public void OnPointerDown(PointerEventData eventData) {
            LastPointerDown = Copy(eventData);
            LatestPointer = eventData;

            // If last click was not too recent:
            var msToLastClick = (Time.unscaledTime - LastPointerUp?.clickTime) * 1000;
            if (LastPointerUp == null || msToLastClick > doubleClickTimeoutInMs) {
                // Start evaluating if it can become a long press, double click or click:
                HandleLongPress().LogOnError();
                DetectClickAndDoubleClick().LogOnError();
            }
        }

        public void OnPointerUp(PointerEventData eventData) {
            LastPointerUp = Copy(eventData);
            SetClickResult(ClickResult.up);
        }

        private void SetClickResult(ClickResult clickResult) {
            var t = pointerUpTask;
            pointerUpTask = new TaskCompletionSource<ClickResult>();
            t.TrySetResult(clickResult);
        }

        private async Task HandleLongPress() {
            // Only execute long press detection if there are listeners registered:
            if (onLongPressStart.IsNullOrEmpty() && onLongPressEnd.IsNullOrEmpty()) { return; }

            var isPointerUpAgain = pointerUpTask.Task;
            await TaskV2.Delay(longPressDurInMs);

            var distanceInPixels = LatestPointer.position - LastPointerDown.position;
            if (distanceInPixels.magnitude > maxPixelDistance) { return; }

            if (!isPointerUpAgain.IsCompleted) {
                SetClickResult(ClickResult.longPress);
                onLongPressStart?.Invoke();
                await pointerUpTask.Task;
                onLongPressEnd?.Invoke();
            }
        }

        public void OnDrag(PointerEventData eventData) { LatestPointer = eventData; }

        private async Task DetectClickAndDoubleClick() {
            // Wait for first click to finish:
            var firstClickResult = await pointerUpTask.Task;
            // If it wasnt a normal click but eg a long press cancel:
            if (firstClickResult != ClickResult.up) { return; }

            // If the user moved the pointer to much, cancel:
            var distanceInPixels = LastPointerUp.position - LastPointerDown.position;
            if (distanceInPixels.magnitude > maxPixelDistance) { return; }

            var pointerUpSecondTime = pointerUpTask;
            await TaskV2.Delay(doubleClickTimeoutInMs);
            // If a second click happened during the wait count it as a double click:
            if (pointerUpSecondTime.Task.IsCompleted) {
                var result = await pointerUpSecondTime.Task;
                if (result == ClickResult.up) { onDoubleClick?.Invoke(); }
            } else { // else it was a normal click
                SetClickResult(ClickResult.click);
                onClick?.Invoke();
            }
        }

        /// <summary> Clones the relevant parts of the pointer event </summary>
        private PointerEventData Copy(PointerEventData e) {
            return new PointerEventData(null) { position = e.position, clickTime = Time.unscaledTime };
        }

    }

}