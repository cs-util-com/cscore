using com.csutil.ui;
using System.Collections;

namespace com.csutil.tests {

    public class Ui21_UserInput : UnitTestMono {

        public override IEnumerator RunTest() {
            var input = GetComponentInChildren<PointerInputHandler>();
            input.onClick.AddListener(() => Log.d("onClick"));
            input.onDoubleClick.AddListener(() => Log.d("onDoubleClick"));
            input.onLongPressStart.AddListener(() => {
                Log.d("onLongPressStart");
                Toast.Show("Now drag me around");
                input.gameObject.AddComponent<UiDragHandler>();
            });
            input.onLongPressEnd.AddListener(() => {
                Log.d("onLongPressEnd");
                input.gameObject.GetComponent<UiDragHandler>().Destroy();
            });
            yield return null;
        }

    }

}