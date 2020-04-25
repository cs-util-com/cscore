using com.csutil.model.immutable;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui16 {

    public class MyFormPresenter : Presenter<IDataStore<MyDataModel>> {

        public GameObject targetView { get; set; }
        public bool simulateUserInput;
        private Dictionary<string, Link> map;

        public async Task OnLoad(IDataStore<MyDataModel> store) {
            map = targetView.GetLinkMap();

            InputField userNameInput = map.Get<InputField>("UserNameInput");
            userNameInput.SubscribeToStateChanges(store, model => model.user.name);
            userNameInput.SetOnValueChangedActionThrottled(newUserName => {
                store.Dispatch(new ChangeUName() { name = newUserName });
            });

            Toggle toggle = map.Get<Toggle>("UserIsHumanCheckbox");
            toggle.SubscribeToStateChanges(store, model => model.user.isHuman);
            toggle.SetOnValueChangedAction(isChecked => {
                store.Dispatch(new ChangeUIsHuman() { isHuman = isChecked });
                return true;
            });

            SetupUserAgeUis(store);

            if (simulateUserInput) {
                throw new NotImplementedException();
            }

            // Await the submit button to allow awaiting theuser completing the form:
            await map.Get<Button>("SaveButton").SetOnClickAction(_ => {
                targetView.GetViewStack().SwitchBackToLastView(targetView);
            });
        }

        private void SetupUserAgeUis(IDataStore<MyDataModel> store) {
            GameObject invalidAgeWarning = map.Get<GameObject>("InvalidAgeWarning");

            InputField userAgeInput = map.Get<InputField>("UserAgeInput");
            userAgeInput.SubscribeToStateChanges(store, model => model.user.age, newAge => {
                userAgeInput.text = newAge > 0 ? "" + newAge : "";
                invalidAgeWarning.SetActiveV2(false);
            });
            userAgeInput.SetOnValueChangedAction(input => {
                var isValidAge = int.TryParse(input, out int newAge) && (0 <= newAge && newAge <= 130);
                if (!isValidAge) {
                    invalidAgeWarning.SetActiveV2(true);
                    return true; // Keep input in UI but dont send it to the store
                }
                Log.d("Text input: " + newAge);
                store.Dispatch(new ChangeUAge() { age = newAge });
                return true;
            });

            Slider slider = map.Get<Slider>("UserAgeSlider");
            slider.SubscribeToStateChanges(store, model => model.user.age);
            slider.SetOnValueChangedActionThrottled(newAge => {
                store.Dispatch(new ChangeUAge() { age = (int)newAge });
            });
        }

    }

}