using System;
using System.Net.Http;

namespace com.csutil.http {

    public class RestFactory {

        public static RestFactory instance { get { return IoC.inject.GetOrAddSingleton<RestFactory>(new object()); } }

        public virtual RestRequest SendGET(Uri uri) {
            return new UriRestRequest(uri).Send(HttpMethod.Get);
        }

    }

}