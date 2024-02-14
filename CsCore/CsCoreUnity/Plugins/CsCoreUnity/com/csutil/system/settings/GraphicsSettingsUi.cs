using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.settings {

    public class GraphicsSettingsUi : Presenter<IGraphicsSettings> {

        public GameObject targetView { get; set; }

        public Task OnLoad(IGraphicsSettings model) {
            var links = targetView.GetLinkMap();

            // VSync toggle
            links.Get<Toggle>("VSyncToggle").isOn = model.UseVSync;
            links.Get<Toggle>("VSyncToggle").SetOnValueChangedAction(x => {
                model.UseVSync = x;
                OnLoad(model);
                return true;
            });

            // Dynamic resolution toggle
            links.Get<Toggle>("DynamicResolutionToggle").isOn = model.UseDynamicResolution;
            links.Get<Toggle>("DynamicResolutionToggle").SetOnValueChangedAction(x => {
                model.UseDynamicResolution = x;
                OnLoad(model);
                return true;
            });

            // Target frame rate
            var targetFrameRateInput = links.Get<Slider>("TargetFrameRateInput");
            targetFrameRateInput.minValue = -1;
            targetFrameRateInput.maxValue = 120;
            targetFrameRateInput.value = model.TargetFrameRate;
            targetFrameRateInput.SetOnValueChangedAction(newFps => {
                if (newFps > 5 && newFps < 120) {
                    model.TargetFrameRate = (int)newFps;
                    OnLoad(model);
                    return true;
                }
                return false;
            });

            // Quality Level
            var qualityLevelDropdown = links.Get<TMP_Dropdown>("QualityLevelDropdown");
            qualityLevelDropdown.SetOptions(model.AvailableQualityLevels, model.QualityLevel);
            qualityLevelDropdown.SetOnValueChangedAction(x => {
                model.QualityLevel = x;
                OnLoad(model);
                return true;
            });

            // Resolution
            var resolutionDropdown = links.Get<TMP_Dropdown>("ResolutionDropdown");
            var options = model.AvailableResolutions.Map(x => $"{x.width}x{x.height}").ToArray();
            resolutionDropdown.SetOptions(options, model.AvailableResolutions.IndexOf(model.CurrentResolution));
            resolutionDropdown.SetOnValueChangedAction(x => {
                model.CurrentResolution = model.AvailableResolutions[x];
                OnLoad(model);
                return true;
            });

            // Shadow Quality
            string[] names = ShadowQuality.GetNames(typeof(ShadowQuality));
            var shadowQualityDropdown = links.Get<TMP_Dropdown>("ShadowQualityDropdown");
            shadowQualityDropdown.SetOptions(names, model.QualityLevel);
            shadowQualityDropdown.SetOnValueChangedAction(i => {
                model.ShadowQuality = EnumUtil.Parse<ShadowQuality>(names[i]);
                OnLoad(model);
                return true;
            });

            // Shadow Distance
            var shadowDistanceInput = links.Get<TMP_InputField>("ShadowDistanceInput");
            shadowDistanceInput.SetTextWithoutNotify(model.ShadowDistance.ToString());
            shadowDistanceInput.SetOnValueChangedAction(x => {
                if (float.TryParse(x, out float shadowRenderDistance) && shadowRenderDistance > 1 && shadowRenderDistance < 5000) {
                    model.ShadowDistance = shadowRenderDistance;
                    OnLoad(model);
                    return true;
                }
                return false;
            });

            // Camera Render Distance
            var cameraRenderDistanceInput = links.Get<TMP_InputField>("CameraRenderDistanceInput");
            cameraRenderDistanceInput.SetTextWithoutNotify(model.CameraRenderDistance.ToString());
            cameraRenderDistanceInput.SetOnValueChangedAction(x => {
                if (float.TryParse(x, out float camRenderDistance) && camRenderDistance > 1 && camRenderDistance < 5000) {
                    model.CameraRenderDistance = camRenderDistance;
                    OnLoad(model);
                    return true;
                }
                return false;
            });

            // DefaultSolverIterations
            var defaultSolverIterationsInput = links.Get<TMP_InputField>("DefaultSolverIterationsInput");
            defaultSolverIterationsInput.SetTextWithoutNotify(model.DefaultSolverIterations.ToString());
            defaultSolverIterationsInput.SetOnValueChangedAction(x => {
                if (int.TryParse(x, out int newSolverIterations) && newSolverIterations > 1 && newSolverIterations < 10) {
                    model.DefaultSolverIterations = newSolverIterations;
                    OnLoad(model);
                    return true;
                }
                return false;
            });

            // AutoSyncTransforms
            links.Get<Toggle>("AutoSyncTransformsToggle").isOn = model.AutoSyncTransforms;
            links.Get<Toggle>("AutoSyncTransformsToggle").SetOnValueChangedAction(x => {
                model.AutoSyncTransforms = x;
                OnLoad(model);
                return true;
            });

            // ReusePhysicsCollisionCallbacks
            links.Get<Toggle>("ReusePhysicsCollisionCallbacksToggle").isOn = model.ReusePhysicsCollisionCallbacks;
            links.Get<Toggle>("ReusePhysicsCollisionCallbacksToggle").SetOnValueChangedAction(x => {
                model.ReusePhysicsCollisionCallbacks = x;
                OnLoad(model);
                return true;
            });

            return Task.CompletedTask;
        }

    }

}