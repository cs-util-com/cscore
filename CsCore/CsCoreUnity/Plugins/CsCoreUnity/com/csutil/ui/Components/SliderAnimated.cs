using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    /// <summary> Will animate the value of the slider to the target value over time </summary>
    [RequireComponent(typeof(Slider))]
    public class SliderAnimated : MonoBehaviour {

        public Slider slider { get; private set; }
        public float speed = 1f;

        public float maxValue {
            get { return slider.maxValue; }
            set { slider.maxValue = value; }
        }

        private float? _targetValue = null;
        [ShowPropertyInInspector]
        public float value {
            get { return slider.value; }
            set {
                // The first time the value is set it will be set directly:
                if (_targetValue == null) { slider.value = value; }
                _targetValue = value;
            }
        }

        private void OnEnable() { slider = GetComponent<Slider>(); }

        private void Update() {
            if (_targetValue.HasValue) {
                var targetValue = _targetValue.Value;
                var currentValue = slider.value;
                var diff = targetValue - currentValue;
                if (diff != 0) {
                    slider.value = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * speed);
                }
            }
        }

    }

}