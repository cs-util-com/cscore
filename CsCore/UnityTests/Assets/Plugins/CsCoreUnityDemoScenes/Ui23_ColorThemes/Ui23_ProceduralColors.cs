using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class Ui23_ProceduralColors : MonoBehaviour {

        public int count = 1000;
        public float range = 3f;
        public bool usePastelColors = false;
        public float pastelWhiteAmount = 0.7f;

        void Start() {
            var template = gameObject.GetChildrenIEnumerable().Single();
            AssertV3.IsNotNull(template, "template");
            var colors = new System.Random().NextRandomColors(count, range: range);
            if (usePastelColors) {
                colors = colors.GetPastelColorVariantFor(pastelWhiteAmount).ToQueue();
            }
            ApplyRandomColor(template, colors.Dequeue());
            while (colors.Any()) {
                var go = gameObject.AddChild(GameObject.Instantiate(template));
                ApplyRandomColor(go, colors.Dequeue());
            }
        }

        private static void ApplyRandomColor(GameObject go, Color32 color) {
            go.GetOrAddComponent<Image>().color = color;
        }

    }

}