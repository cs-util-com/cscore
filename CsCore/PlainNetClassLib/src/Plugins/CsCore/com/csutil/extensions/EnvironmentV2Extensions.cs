using System;

namespace com.csutil {

    public static class EnvironmentV2Extensions {

        public static bool IsWindows(this EnvironmentV2 self) {
            var sysInfo = self.systemInfo;
            bool IsWindows = sysInfo.osPlatform.StartsWith("Win", StringComparison.InvariantCultureIgnoreCase);
            return IsWindows;
        }

    }

}