using System.Collections.Generic;
using System.Linq;
using ReuseScroller;

namespace com.csutil {

    public static class ListUiExtensions {

        public static void SetListData<T>(this BaseController<T> list, IEnumerable<T> content, bool isReset = true) {
            list.SetListData(content.ToList(), isReset);
        }

        public static V GetParentListUiController<T, V>(this BaseCell<T> cell) where V : BaseController<T> {
            var result = cell.GetComponentInParent<V>(includeInactive: true);
            if (result.IsNullOrDestroyed()) { // Check if null:
                Log.e("Parent list UI controller was not found for " + cell.gameObject.FullQualifiedName(), cell.gameObject);
            }
            return result;
        }

    }

}