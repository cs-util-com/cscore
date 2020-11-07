using ReuseScroller;
using System;

namespace com.csutil.tests {

    internal class UserListController : BaseController<Task5_CRUD.MyUser> {

        public Action<Task5_CRUD.MyUser> OnUserEntryClicked;

    }

}