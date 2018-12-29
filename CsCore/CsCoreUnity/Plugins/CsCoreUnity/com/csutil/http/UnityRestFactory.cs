using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace com.csutil.http {

    public class UnityRestFactory : RestFactory {

        public override RestRequest SendGET(Uri uri) {
            return new UnityRestRequest(UnityWebRequest.Get(uri));
        }

    }

}
