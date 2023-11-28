using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.csutil.webgl;

public class WebGL02_ShareManager : MonoBehaviour
{

    public GameObject shareManager;
    public GameObject urlInput;
    public GameObject titleInput;
    public GameObject fileNameInput;
    public GameObject messageInput;
    public GameObject fileInput;

    public GameObject canShareIndicato;

    // Start is called before the first frame update
    void Start()
    {
        canShare();
    }

    public void onShareUrl() {
        Debug.Log("Clicked ShareUrl");
        string message = messageInput.GetComponent<InputField>().text;
        string url = urlInput.GetComponent<InputField>().text;
        string title = titleInput.GetComponent<InputField>().text;
        Debug.Log("File Share: " + title + ", " + message + ", " + url);

        shareManager.GetComponent<ShareManager>().share(title,message,url,"","");
    }

    public void onShareFile() {
        Debug.Log("Clicked ShareFile");
        string title = titleInput.GetComponent<InputField>().text;
        string message = messageInput.GetComponent<InputField>().text;
        string file = fileInput.GetComponent<InputField>().text;
        string fileName = fileNameInput.GetComponent<InputField>().text;
        Debug.Log("File Share: " + title + ", " + message + ", " + file + ", " + fileName);
        shareManager.GetComponent<ShareManager>().share(title, message, "", file,fileName);
        
    }

    public void canShare() {
        Debug.Log("Clicked Can Share");
        ColorBlock buttonColor = canShareIndicato.GetComponent<Button>().colors; 
        
        if (shareManager.GetComponent<ShareManager>().canShare()) {
            buttonColor.normalColor = Color.green;
            canShareIndicato.GetComponent<Button>().colors = buttonColor;

        } else  {
            buttonColor.normalColor = Color.red;
            canShareIndicato.GetComponent<Button>().colors = buttonColor;

        }


    }
}
