using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.async {

    public class AsyncVoidDetector {

        [Fact]
        public void EnsureNoAsyncVoidTests() {
            // This helper will detect any methods of type async void that are 
            // found in CsCore or the CsCore tests so that they can be 
            // changed from "async void" to "async Task"
            AssertNoAsyncVoidMethods(GetType().Assembly); // Check in all tests
            AssertNoAsyncVoidMethods(typeof(EventBus).Assembly); // check in  library 
        }

        private static void AssertNoAsyncVoidMethods(Assembly assembly) {
            var messages = GetAsyncVoidMethods(assembly)
                .Select(method => string.Format("'{0}.{1}' is an async void method.", method.DeclaringType.Name, method.Name))
                .ToList();
            Assert.False(messages.Any(), "Async void methods found! \n" + string.Join("\n", messages));
        }

        private static IEnumerable<MethodInfo> GetAsyncVoidMethods(Assembly assembly) {
            var anyAttrCombi = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
                         | BindingFlags.Static | BindingFlags.DeclaredOnly;
            return assembly.GetLoadableTypes()
              .SelectMany(type => type.GetMethods(anyAttrCombi))
              .Where(method => method.HasAttribute<AsyncStateMachineAttribute>())
              .Where(method => method.ReturnType == typeof(void));
        }

    }

}