using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil.http {

    public static class RestRequestExtensions {

        public static RestRequest WithJsonContent(this RestRequest self, string jsonContent) {
            return self.WithTextContent(jsonContent, Encoding.UTF8, "application/json");
        }

    }

}
