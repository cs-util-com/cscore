using System.Linq;
using com.csutil.math;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class Ui23_ProceduralColors : MonoBehaviour {

        void Start() {
            var template = gameObject.GetChildrenIEnumerable().Single();
            AssertV3.IsNotNull(template, "template");
            var colors = new System.Random().NextRandomColors(1000);
            //colors = colors.GetPastelColorVariantFor().ToQueue();
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