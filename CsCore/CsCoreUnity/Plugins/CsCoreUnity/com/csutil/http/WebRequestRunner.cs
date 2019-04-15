using UnityEngine;

namespace com.csutil.http {

    public class WebRequestRunner : MonoBehaviour {

        public static WebRequestRunner GetInstance(object caller) { return IoC.inject.GetOrAddComponentSingleton<WebRequestRunner>(caller); }

        private void OnEnable() { }

        private void OnDisable() { }

    }

}
