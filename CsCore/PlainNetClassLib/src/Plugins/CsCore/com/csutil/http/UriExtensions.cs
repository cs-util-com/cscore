using System.Net.Http;
using com.csutil.http;

namespace System {
    public static class UriExtensions {

        public static RestRequest SendGET(this Uri self) { 
            return RestFactory.instance.SendGET(self); 
        }

    }
}