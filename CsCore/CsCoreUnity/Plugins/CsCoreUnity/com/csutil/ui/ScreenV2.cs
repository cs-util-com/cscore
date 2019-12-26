using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public class ScreenV2 : MonoBehaviour {

        public const string EVENT_WINDOW_RESIZE = "EventWindowResize";

        private static int _w = 0;
        public static int width {
            get { if (_w == 0) { Instance(); return Screen.width; } return _w; }
        }

        private static int _h = 0;
        public static int height {
            get { if (_h == 0) { Instance(); return Screen.height; } return _h; }
        }

        private static ScreenV2 Instance() { return IoC.inject.GetOrAddComponentSingleton<ScreenV2>(new object()); }

        private void Start() {
            SaveScreenSize(); // force initial init
            this.ExecuteRepeated(checkIfWindowSizeChanged, delayInMsBetweenIterations: 100);
        }

        private bool checkIfWindowSizeChanged() {
            if (height != Screen.height || width != Screen.width) {
                SaveScreenSize();
                EventBus.instance.Publish(EVENT_WINDOW_RESIZE, width, height);
            }
            return true;
        }

        private static void SaveScreenSize() { _h = Screen.height; _w = Screen.width; }

        public static Rect GetScreenRect() {
            var screen = new Rect();
            screen.xMin = 0;
            screen.xMax = ScreenV2.width;
            screen.yMin = 0;
            screen.yMax = ScreenV2.height;
            return screen;
        }

    }

}
