using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace com.csutil.http {

    public class UnityRestRequest : RestRequest {

        private UnityWebRequest request;

        public UnityRestRequest(UnityWebRequest request) { this.request = request; }

        public Task<T> GetResult<T>(Action<T> onResult = null) {
            throw new NotImplementedException();
        }

    }

}