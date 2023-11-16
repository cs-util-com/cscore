using System.Collections.Generic;
using System.Linq;
using ReuseScroller;

namespace com.csutil.ui {

    public static class ListUiExtensions {

        public static void SetListData<T>(this BaseController<T> list, IEnumerable<T> content) {
            list.SetListData(content.ToList());
        }

        public static V GetParentListUiController<T, V>(this BaseCell<T> cell) where V : BaseController<T> {
            return (V)cell.GetParentListUiController();
        }

        public static BaseController<T> GetParentListUiController<T>(this BaseCell<T> cell) {
            return cell.GetComponentInParent<BaseController<T>>();
        }

    }

}