using com.csutil.ui;
using System.Collections;

namespace com.csutil.tests {

    public class Ui21_UserInput : UnitTestMono {

        public override IEnumerator RunTest() {
            var input = GetComponentInChildren<PointerInputHandler>();
            input.onClick.AddListener(() => Toast.Show("onClick"));
            input.onDoubleClick.AddListener(() => Toast.Show("onDoubleClick"));
            input.onLongPressStart.AddListener(() => {
                Toast.Show("Now drag me around");
                input.gameObject.AddComponent<UiDragHandler>();
            });
            input.onLongPressEnd.AddListener(() => {
                Toast.Show("onLongPressEnd");
                input.gameObject.GetComponent<UiDragHandler>().Destroy();
            });
            yield return null;
        }

    }

}