using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace com.csutil.editor {
    class EditorCoroutineRunner {

        public static void StartCoroutine(IEnumerator update, Action end = null) {
            EditorApplication.CallbackFunction closureCallback = null;
            closureCallback = () => {
                try {
                    if (!update.MoveNext()) {
                        end?.Invoke();
                        EditorApplication.update -= closureCallback;
                    }
                }
                catch (Exception ex) {
                    end?.Invoke();
                    EditorApplication.update -= closureCallback;
                }
            };
            EditorApplication.update += closureCallback;
        }

    }
}
