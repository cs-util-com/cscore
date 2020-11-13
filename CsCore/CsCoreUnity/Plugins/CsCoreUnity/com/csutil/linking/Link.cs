using UnityEngine;

namespace com.csutil {

    public class Link : MonoBehaviour {

        public string id;
        public bool syncWithGOName = true;

        private void OnValidate() {
            if (id.IsNullOrEmpty() && !name.ToLowerInvariant().Contains("gameobject")) { id = name; }
            if (!id.IsNullOrEmpty() && syncWithGOName && name != id) { name = id; }
        }

        internal void SetId(string id) {
            if (id.IsNullOrEmpty()) { throw Log.e("Invalid Link.id (null or empty)", gameObject); }
            this.id = id;
        }

    }

}
