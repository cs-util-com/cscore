using System.Threading.Tasks;
using com.csutil.model.jsonschema;
using com.csutil.ui;
using UnityEngine;

namespace com.csutil.settings {

    public interface IGraphicsSettings {

        [DropDown(dropdownId = "Quality Level")]
        string[] AvailableQualityLevels { get; }

        [DropDown(dropdownId = "Quality Level")]
        int QualityLevel { get; set; }

        [MinMaxRange(0, 120)]
        int TargetFrameRate { get; set; }

        [Description("On mobile On is the default and should result in better performance")]
        bool UseVSync { get; set; }

        [DropDown(dropdownId = "Resolution")]
        Resolution[] AvailableResolutions { get; }

        [DropDown(dropdownId = "Resolution")]
        Resolution CurrentResolution { get; set; }

        bool UseDynamicResolution { get; set; }

        ShadowQuality ShadowQuality { get; set; }

        float ShadowDistance { get; set; }

        float CameraRenderDistance { get; set; }

        /// <summary> Off might result in better performance. See https://unity.com/how-to/enhanced-physics-performance-smooth-gameplay#disable-automatic-transform-syncing </summary>
        bool AutoSyncTransforms { get; set; }

        /// <summary> On might result in better performance. See https://unity.com/how-to/enhanced-physics-performance-smooth-gameplay#reuse-collision-callbacks </summary>
        bool ReusePhysicsCollisionCallbacks { get; set; }

        /// <summary> Small values (eg 2) should result in better performance. See https://unity.com/how-to/enhanced-physics-performance-smooth-gameplay#modify-solver-iterations </summary>
        int DefaultSolverIterations { get; set; }

    }

    public class GraphicsSettings : MonoBehaviour, IGraphicsSettings {

        public Camera targetCamera;

        public Task Show(ViewStack viewStack) {
            return new GraphicsSettingsUi().ShowModelInView(this, "GraphicsSettingsUi", viewStack);
        }

        private void OnEnable() {
            if (targetCamera == null) {
                Log.e("No target camera set, using Camera.main", gameObject);
                targetCamera = Camera.main;
            }
            AssertV3.IsNotNull(targetCamera, "targetCamera");
        }

        // Resolutions
        public Resolution[] AvailableResolutions => Screen.resolutions;
        public Resolution CurrentResolution { get => Screen.currentResolution; set => Screen.SetResolution(value.width, value.height, Screen.fullScreen); }

        public bool UseDynamicResolution { get => targetCamera.allowDynamicResolution; set => targetCamera.allowDynamicResolution = value; }

        /// <summary> On mobile On should be faster </summary>
        public bool UseVSync { get => QualitySettings.vSyncCount > 0; set => QualitySettings.vSyncCount = value ? 1 : 0; }

        public int TargetFrameRate { get => ApplicationV2.targetFrameRateV2; set => ApplicationV2.targetFrameRateV2 = value; }

        public string[] AvailableQualityLevels => QualitySettings.names;
        public int QualityLevel { get => QualitySettings.GetQualityLevel(); set => QualitySettings.SetQualityLevel(value, true); }

        public ShadowQuality ShadowQuality { get => QualitySettings.shadows; set => QualitySettings.shadows = value; }

        public float ShadowDistance { get => QualitySettings.shadowDistance; set => QualitySettings.shadowDistance = value; }

        public float CameraRenderDistance { get => targetCamera.farClipPlane; set => targetCamera.farClipPlane = value; }

        public bool AutoSyncTransforms { get => Physics.autoSyncTransforms; set => Physics.autoSyncTransforms = value; }
        public bool ReusePhysicsCollisionCallbacks { get => Physics.reuseCollisionCallbacks; set => Physics.reuseCollisionCallbacks = value; }

        public int DefaultSolverIterations { get => Physics.defaultSolverIterations; set => Physics.defaultSolverIterations = value; }

    }

}