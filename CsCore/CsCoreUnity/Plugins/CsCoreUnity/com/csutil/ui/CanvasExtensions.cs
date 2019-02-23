using com.csutil.ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public static class CanvasExtensions {

        public static void CopyCanvasSettingsFrom(this Canvas self, Canvas canvasToUseSettingsFrom) {
            // TODO
        }

        public static bool DestroyUiScreen(this Canvas self) { return self.gameObject.Destroy(); }

    }

}
