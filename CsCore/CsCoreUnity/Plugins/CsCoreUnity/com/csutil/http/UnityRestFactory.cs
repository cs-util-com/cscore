using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using UnityEngine.Networking;

namespace com.csutil.http {

    public class UnityRestFactory : RestFactory {

        public override RestRequest SendRequest(Uri uri, HttpMethod method) {
            if (method.ToString() == "GET") {
                return new UnityRestRequest(UnityWebRequest.Get(uri));
            }
            throw new NotImplementedException("Not yet implemented for http method " + method);
        }

    }

}
