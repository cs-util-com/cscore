using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil {

    public static class ImageExtensions {

        public static Sprite ToSprite(this Texture2D self) { return Sprite.Create(self, new Rect(0, 0, self.width, self.height), new Vector2(0.5f, 0.5f)); }

    }

}
