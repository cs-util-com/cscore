using com.csutil.injection;
using com.csutil.io;

namespace com.csutil {

    public static class TestHelperExtensionMethods {

        /// <summary> An example how to make AppSecrets easily available in unit tests, the developer can then place a
        /// "DevEnvSecrets.txt" in his workspace root folder and make all secrets available to the unit tests this way </summary>
        public static AppSecrets GetAppSecrets(this Injector injector, string fileName = "cscore-DevEnvSecrets.txt") {
            return injector.GetOrAddSingleton<AppSecrets>(null, () => new DevEnvSecretsForLocalTesting(fileName));
        }

    }

}