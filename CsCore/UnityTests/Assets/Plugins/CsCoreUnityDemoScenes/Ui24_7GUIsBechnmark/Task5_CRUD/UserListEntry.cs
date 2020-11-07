using ReuseScroller;
using UnityEngine.UI;

namespace com.csutil.tests {

    internal class UserListEntry : BaseCell<Task5_CRUD.MyUser> {

        public Text fullName;
        public Button entrySelector;

        public override void UpdateContent(Task5_CRUD.MyUser user) {
            fullName.text = user.ToString();
            entrySelector.SetOnClickAction(delegate {
                GetComponentInParent<UserListController>().OnUserEntryClicked(user);
            });
        }

    }

}