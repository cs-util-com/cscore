using System;
using System.Collections;
using Xunit;

namespace com.csutil.integrationTests.io {

    public class EnvironmentVariablesTests {

        public EnvironmentVariablesTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void TestRawEnvironmentVariablesAccess() {

            var cscoretestenvvar1 = "CsCoreTestEnvVar1";
            EnvironmentV2.instance.SetEnvironmentVariable(variable: cscoretestenvvar1, value: "123");
            Assert.Equal("123", EnvironmentV2.instance.GetEnvironmentVariable(variable: cscoretestenvvar1));

            var cscoretestenvvar2 = "CsCoreTestEnvVar2";
            EnvironmentV2.instance.SetEnvironmentVariable(variable: cscoretestenvvar2, value: "456", target: EnvironmentVariableTarget.Process);

            var allEnvironmentVariables = EnvironmentV2.instance.GetEnvironmentVariables();
            Assert.True(allEnvironmentVariables.Contains(cscoretestenvvar1));
            Assert.True(allEnvironmentVariables.Contains(cscoretestenvvar2));
            foreach (DictionaryEntry variable in allEnvironmentVariables) {
                Log.d($"Found variable {variable.Key} = {variable.Value}");
            }

        }

    }

}