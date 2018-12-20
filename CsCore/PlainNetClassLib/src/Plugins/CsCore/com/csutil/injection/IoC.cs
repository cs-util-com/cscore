using System;
using com.csutil.injection;

namespace com.csutil {
    public class IoC {

        public static Injector inject = Injector.newInjector(EventBus.instance);

    }
}