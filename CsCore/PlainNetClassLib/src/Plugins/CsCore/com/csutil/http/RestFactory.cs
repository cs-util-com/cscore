using System;

namespace com.csutil.http {
    public class RestFactory {
        public static RestFactory instance = new RestFactory();

        public virtual RestRequest SendGET(Uri uri) { return uri.SendGET(); }
    }
}