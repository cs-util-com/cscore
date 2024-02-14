using System.Threading.Tasks;
using com.csutil;
using com.csutil.settings;
using UnityEngine;
using UnityEngine.UI;

public class Ui31_GraphicsSettingsUi : MonoBehaviour {

    public GraphicsSettings graphicsSettings;

    void Start() {
        var links = gameObject.GetLinkMap();
        links.Get<Button>("OpenGraphicsSettingsButton").SetOnClickAction(OpenGraphicsSettings);
    }

    private async Task OpenGraphicsSettings(GameObject button) {
        await graphicsSettings.Show(button.GetViewStack());
    }

}