using System;

namespace com.csutil {

    public static class EnvironmentV2Extensions {

        public static bool IsWindows(this EnvironmentV2 self) {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || PLATFORM_STANDALONE_WIN
            return true;
#endif
            var sysInfo = self.systemInfo;
            bool IsWindows = sysInfo.osPlatform.StartsWith("Win", StringComparison.InvariantCultureIgnoreCase);
            return IsWindows;
        }

    }

}