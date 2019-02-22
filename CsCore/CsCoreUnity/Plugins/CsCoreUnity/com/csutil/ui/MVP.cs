using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://en.wikipedia.org/wiki/Model–view–presenter
public interface Presenter<T> {

    IEnumerator LoadModelIntoView(T model, GameObject view);

    IEnumerator Unload();

}
