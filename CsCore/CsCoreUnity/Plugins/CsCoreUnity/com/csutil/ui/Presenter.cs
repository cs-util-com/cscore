using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.ui {

    // https://en.wikipedia.org/wiki/Model–view–presenter
    public interface Presenter<T> {

        IEnumerator LoadModelIntoViewCoroutine(T model, GameObject view);

        IEnumerator Unload();

    }

    public static class PresenterExtensions {

        /// <summary> Connects a model with a view </summary>
        /// <returns> A task that can be awaited on, that returns the fully setup presenter </returns>
        public static Task<Presenter<T>> LoadModelIntoView<T>(this Presenter<T> self, T model, GameObject view) {
            return view.GetComponent<MonoBehaviour>().StartCoroutineAsTask(UnloadAndLoadNew(self, model, view), () => self);
        }

        private static IEnumerator UnloadAndLoadNew<T>(Presenter<T> self, T model, GameObject view) {
            yield return self.Unload();
            yield return self.LoadModelIntoViewCoroutine(model, view);
        }
    }

}