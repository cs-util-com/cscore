using System;
using System.Net.Http;
using com.csutil.http;

namespace com.csutil {

    public static class UriExtensions {

        public static RestRequest SendGET(this Uri self) { return RestFactory.instance.SendRequest(self, HttpMethod.Get); }

        public static RestRequest SendPOST(this Uri self) { return RestFactory.instance.SendRequest(self, HttpMethod.Post); }

        public static RestRequest SendPUT(this Uri self) { return RestFactory.instance.SendRequest(self, HttpMethod.Put); }

        public static RestRequest SendDELETE(this Uri self) { return RestFactory.instance.SendRequest(self, HttpMethod.Delete); }

        public static RestRequest SendHEAD(this Uri self) { return RestFactory.instance.SendRequest(self, HttpMethod.Head); }

        public static RestRequest SendOPTIONS(this Uri self) { return RestFactory.instance.SendRequest(self, HttpMethod.Options); }

        public static RestRequest SendRequest(this Uri self, HttpMethod method) { return RestFactory.instance.SendRequest(self, method); }

    }

}