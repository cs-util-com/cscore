using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace System.Net.NetworkInformation {
    public static class PingCompatExtensions {

        public static Task<PingReply> SendPingAsync(this Ping self, string hostNameOrAddress, int timeout) {
            return Task.Factory.StartNew(() => { return self.Send(hostNameOrAddress, timeout); });
        }

    }
}
