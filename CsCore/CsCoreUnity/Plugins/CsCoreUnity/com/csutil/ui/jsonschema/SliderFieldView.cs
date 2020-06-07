using com.csutil.model.mtvmtv;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class SliderFieldView : FieldView {

        public Slider slider;
        public Text valueDisplay;

        protected override Task Setup(string fieldName, string fullPath) {
            slider.interactable = field.readOnly != true;
            slider.minValue = field.minimum.Value;
            slider.maxValue = field.maximum.Value;
            slider.wholeNumbers = field.GetJTokenType() == JTokenType.Integer;
            return Task.FromResult(true);
        }
    }

}