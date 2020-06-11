using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public class Link : MonoBehaviour {
        public string id;

        private void OnValidate() {
            if (id.IsNullOrEmpty() && !name.ToLowerInvariant().Contains("gameobject")) { id = name; }
        }

        internal void SetId(string id) {
            if (id.IsNullOrEmpty()) { throw Log.e("Link.id cant be set to null", gameObject); }
            this.id = id;
        }

    }

}
