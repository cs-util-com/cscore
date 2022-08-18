using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.ui.debug;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class Ui29_KeyValueStoreMonitor : UnitTestMono {

        public override IEnumerator RunTest() { yield return RunTestTask().AsCoroutine(); }

        private async Task RunTestTask() {

            var store = new ObservableKeyValueStore(new InMemoryKeyValueStore());

            await store.ShowDebugUiScreen();

            var ui = gameObject.GetLinkMap();

            var key = ui.Get<InputField>("KeyInput");
            var value = ui.Get<InputField>("ValueInput");

            var userClickedOnSet = ui.Get<Button>("SetButton").SetOnClickActionAsync(async delegate {
                var keyText = key.text;
                var valueText = value.text;
                var replacedValue = await store.Set(keyText, valueText);
                Log.d($"For key '{keyText}' now the value us set to '{valueText}' (where before it was set to '{replacedValue}')");
            });
            var userClickedOnRemove = ui.Get<Button>("RemoveButton").SetOnClickActionAsync(async delegate {
                await store.Remove(key.text);
            });

            var userClickedOnClearAll = ui.Get<Button>("ClearAllButton").SetOnClickActionAsync(async delegate {
                await store.RemoveAll();
            });

            if (simulateUserInput) {
                const string SOME_KEY_1 = "Key 1";
                const string SOME_VALUE_1 = "My Value 1";
                const string SOME_VALUE_2 = "My Value 2";

                key.text = SOME_KEY_1;
                value.text = SOME_VALUE_1;
                AssertV2.AreEqual(1, userClickedOnSet.Count());
                AssertV2.AreEqual(0, userClickedOnSet.Filter(t => t.IsCompleted).Count());
                await SimulateButtonClickOn("SetButton", userClickedOnSet);
                AssertV2.AreEqual(2, userClickedOnSet.Count());
                AssertV2.AreEqual(1, userClickedOnSet.Filter(t => t.IsCompleted).Count());
                AssertV2.AreEqual(SOME_VALUE_1, await store.Get<string>(SOME_KEY_1, null));

                value.text = SOME_VALUE_2;
                await SimulateButtonClickOn("SetButton", userClickedOnSet);
                AssertV2.AreEqual(3, userClickedOnSet.Count);
                AssertV2.AreEqual(SOME_VALUE_2, await store.Get<string>(SOME_KEY_1, null));

                AssertV2.IsTrue(await store.ContainsKey(SOME_KEY_1), "SOME_KEY_1 NOT found in key value store");
                await SimulateButtonClickOn("RemoveButton", userClickedOnRemove);
                AssertV2.IsFalse(await store.ContainsKey(SOME_KEY_1), "SOME_KEY_1 STILL found in key value store");
                AssertV2.AreEqual(null, await store.Get<string>(SOME_KEY_1, null));

                await store.Set("a", "a");
                await store.Set("b", "b");
                AssertV2.AreEqual(2, (await store.GetAllKeys()).Count());
                await SimulateButtonClickOn("ClearAllButton", userClickedOnClearAll);
                AssertV2.AreEqual(0, (await store.GetAllKeys()).Count());
            }

        }

    }

}