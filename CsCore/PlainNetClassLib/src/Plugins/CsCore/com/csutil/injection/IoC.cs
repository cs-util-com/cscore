using System;
using com.csutil.injection;

namespace com.csutil {
    public static class IoC {

        public static Injector inject = Injector.newInjector(EventBus.instance);

        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-constructors
        static IoC() {
            // Log.d("IoC used the first time..");
        }

    }
}