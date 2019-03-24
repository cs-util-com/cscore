using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.csutil.ui {

    // https://en.wikipedia.org/wiki/Model–view–presenter
    public interface Presenter<T> {

        IEnumerator LoadModelIntoViewAsync(T model, GameObject view);

        IEnumerator Unload();

    }

    public static class PresenterExtensions {

        public static Coroutine LoadModelIntoView<T>(this Presenter<T> self, T model, GameObject view) {
            return view.GetComponent<MonoBehaviour>().StartCoroutine(self.LoadModelIntoViewAsync(model, view));
        }

    }

}