using com.csutil.injection;
using com.csutil.io;

namespace com.csutil {

    public static class TestHelperExtensionMethods {

        public static AppSecrets GetAppSecrets(this Injector injector, string fileName = "cscore-DevEnvSecrets.txt") {
            return injector.GetOrAddSingleton<AppSecrets>(null, () => new DevEnvSecretsForLocalTesting(fileName));
        }

    }

}