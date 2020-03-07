using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.localization {

    /// <summary> For static Unity UI text elements this can be added to automatically localize the Text to the users language </summary>
    [RequireComponent(typeof(Text))]
    public class TextLocalizer : MonoBehaviour {

        private void Start() { GetComponent<Text>().textLocalized(GetComponent<Text>().text); }

    }

}