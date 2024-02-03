using System.Collections;
using UnityEngine;

namespace com.csutil.tests.ui {

    public class SomeObjectSpawnLogic : MonoBehaviour {

        public bool usePooling = true;
        public string prefabName = "MyTemplate1";

        private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        private WaitForSeconds waitForSeconds = new WaitForSeconds(3);

        IEnumerator Start() {
            var parentForInstances = new GameObject(prefabName + " Instances");
            for (int i = 0; i < 100; i++) {
                for (int j = -30; j < 30; j++) {
                    for (int k = 0; k < 30; k += 2) {
                        
                        // Spawn the object (using pooling if enabled):
                        GameObject obj = usePooling ? ResourcesV2.LoadPrefabPooled(prefabName) : ResourcesV2.LoadPrefab(prefabName);
                        
                        // Set the object parent and pose as usual:
                        obj.transform.SetParent(parentForInstances.transform);
                        obj.transform.position = new Vector3(j * 1.5f, 0, i % 20 + k);
                        
                        // Random return to pool time between 2 and 4 seconds:
                        obj.GetOrAddComponent<DespawnAfterXSec>().secondsUntilDespawn = 2f + Random.value * 2f;
                        
                    }
                    yield return waitForEndOfFrame;
                }
                yield return waitForSeconds;
            }
        }

    }

}