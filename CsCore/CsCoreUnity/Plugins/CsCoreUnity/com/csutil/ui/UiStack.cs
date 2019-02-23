using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil.ui {

    public class UiStack : MonoBehaviour {

        public static Canvas GetRootCanvasFor(GameObject gameObject) {
            var c = gameObject.GetComponentInParent<Canvas>();
            return c != null ? c.rootCanvas : gameObject.GetComponent<Canvas>();
        }

    }

}
