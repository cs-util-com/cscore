using System;
using System.Collections;
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
                    EditorApplication.update -= closureCallback;
                    throw ex;
                }
            };
            EditorApplication.update += closureCallback;
        }

    }

}
