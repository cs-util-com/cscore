using com.csutil.model.immutable;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    public class Ui16_InputForm : UnitTestMono {

        public MyDataModel initialModel = new MyDataModel() { user = new MyUser() };

        public override IEnumerator RunTest() {

            IDataStore<MyDataModel> store = new DataStore<MyDataModel>(MainReducer, initialModel);

            var map = gameObject.GetLinkMap();

            InputField userNameInput = map.Get<InputField>("UserNameInput");
            userNameInput.SubscribeToStateChanges(store, model => model.user.name);
            userNameInput.SetOnValueChangedActionThrottled(newUserName => {
                store.Dispatch(new ChangeUName() { name = newUserName });
            });

            var userAgeInput = map.Get<InputField>("UserAgeInput");
            userAgeInput.SubscribeToStateChanges(store, model => "" + model.user.age);
            userAgeInput.SetOnValueChangedAction(input => {
                var isValidAge = int.TryParse(input, out int newAge) && (0 <= newAge && newAge <= 130);
                if (isValidAge) { store.Dispatch(new ChangeUAge() { age = newAge }); }
                map.Get<GameObject>("InvalidAgeWarning").SetActiveV2(!isValidAge);
                return isValidAge;
            });

            var toggle = map.Get<Toggle>("UserIsHumanCheckbox");
            toggle.SubscribeToStateChanges(store, model => model.user.isHuman);
            toggle.SetOnValueChangedAction(isChecked => {
                store.Dispatch(new ChangeUIsHuman() { isHuman = isChecked });
                return true;
            });

            var slider = map.Get<Slider>("UserAgeSlider");
            slider.SubscribeToStateChanges(store, model => model.user.age);
            slider.SetOnValueChangedActionThrottled(newAge => {
                store.Dispatch(new ChangeUAge() { age = (int)newAge });
            });

            if (simulateUserInput) {
                throw new NotImplementedException();
            }

            yield return null;

        }

        private MyDataModel MainReducer(MyDataModel previousState, object action) {
            bool changed = false;
            var user = previousState.user.Mutate(action, ReduceUser, ref changed);
            if (changed) { return new MyDataModel() { user = user }; }
            return previousState;
        }

        private MyUser ReduceUser(MyUser user, object a) {
            if (a is ChangeUName c1) { return user.DeepCopy(newUser => newUser.name = c1.name); }
            if (a is ChangeUIsHuman c2) { return user.DeepCopy(newUser => newUser.isHuman = c2.isHuman); }
            if (a is ChangeUAge c3) { return user.DeepCopy(newUser => newUser.age = c3.age); }
            return user;
        }

        public class ChangeUName { public string name; }
        public class ChangeUIsHuman { public bool isHuman; }
        public class ChangeUAge { public int age; }

        [Serializable]
        public class MyDataModel {
            public MyUser user;
        }

        [Serializable]
        public class MyUser {
            public string name;
            public int age;
            internal bool isHuman;
        }

    }


}