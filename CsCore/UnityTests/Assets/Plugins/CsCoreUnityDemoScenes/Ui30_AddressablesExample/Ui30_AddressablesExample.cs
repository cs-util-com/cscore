using System.Collections;
using System.Threading.Tasks;
using com.csutil;
using UnityEngine;

public class Ui30_AddressablesExample : MonoBehaviour {

    public string somePrefabName = "MyPrefab123.prefab";

    IEnumerator Start() {
        yield return AddressableLoadingTest().AsCoroutine();
    }

    private async Task AddressableLoadingTest() {
        var go = await ResourcesV2.LoadPrefabV2(somePrefabName);
        gameObject.AddChild(go);
        AssertV2.IsNotNull(go, "go");
        Log.d("Go" + go);
    }

}