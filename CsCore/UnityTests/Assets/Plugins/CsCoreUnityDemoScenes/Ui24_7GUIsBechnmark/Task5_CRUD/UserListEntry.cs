using ReuseScroller;
using UnityEngine.UI;

namespace com.csutil.tests {

    internal class UserListEntry : BaseCell<Task5_CRUD.MyUser> {

#pragma warning disable 0649 // Variable is never assigned to, and will always have its default value

        public Text fullName; // Assigned through Unity UI
        public Button entrySelector; // Assigned through Unity UI

#pragma warning restore 0649 // Variable is never assigned to, and will always have its default value


        public override void UpdateContent(Task5_CRUD.MyUser user) {
            fullName.text = user.ToString();
            entrySelector.SetOnClickAction(delegate {
                GetComponentInParent<UserListController>().OnUserEntryClicked(user);
            });
        }

    }

}