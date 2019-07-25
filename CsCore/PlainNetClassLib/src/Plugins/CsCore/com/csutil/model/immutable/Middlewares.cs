using System;

namespace com.csutil.model.immutable {
    public class Middlewares {

        // See e.g. https://github.com/reduxjs/redux-thunk/blob/master/src/index.js
        public static Middleware<T> NewThunkMiddleware<T>(object extraArgument = null) {
            return (DataStore<T> store) => {
                return (Dispatcher innerDispatcher) => {
                    Dispatcher thunkDispatcher = (action) => {
                        if (action is Delegate d) { return d.DynamicInvokeV2(store, extraArgument); }
                        return innerDispatcher(action);
                    };
                    return thunkDispatcher;
                };
            };
        }

    }
}