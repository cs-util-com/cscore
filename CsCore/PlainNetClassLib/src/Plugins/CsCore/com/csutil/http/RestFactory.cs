using System;
using System.Net.Http;

namespace com.csutil.http {

    public class RestFactory {

        public static RestFactory instance { get { return IoC.inject.GetOrAddSingleton<RestFactory>(new object()); } }

        public virtual RestRequest SendRequest(Uri uri, HttpMethod method) {
            return new UriRestRequest(uri).Send(method);
        }

    }

}