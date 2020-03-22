using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    public class Ui13_Localization : MonoBehaviour {

        /// <summary> Try out 0, 1 and 2 </summary>
        public int daysLeft = 1;

        void Start() {
            var map = gameObject.GetLinkMap();
            Text text1 = map.Get<Text>("DynamicText1");

            // Instead of using text1.text = "Abc" you can use text1.textLocalized.. to automatically use the I18n logic:
            text1.textLocalized("I am a localized text, today is {0}, so there are {1} days left till the release!", DateTime.Now, daysLeft);
        }

    }

}