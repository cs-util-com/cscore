using com.csutil.model.mtvmtv;
using System.Threading.Tasks;

namespace com.csutil.ui.mtvmtv {

    public class ViewModelFieldView : FieldView {

        protected override Task Setup(string fieldName, string fullPath) {
            if (fullPath.IsNullOrEmpty()) {
                gameObject.name = "root";
                mainLink.id = "root";
            }
            return Task.FromResult(true);
        }

    }

}