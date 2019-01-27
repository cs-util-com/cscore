using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public class Link : MonoBehaviour {
        public string id;

        private void OnValidate() {
            if (id.IsNullOrEmpty() && !name.ToLower().Contains("gameobject")) { id = name; }
        }

    }

}
