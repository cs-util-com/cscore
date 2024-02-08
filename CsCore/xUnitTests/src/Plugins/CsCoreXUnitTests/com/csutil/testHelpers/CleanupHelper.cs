using System;
using System.Collections.Generic;

namespace com.csutil {

    /// <summary> Triggers all its cleanup tasks when it is disposed, can be used in a unit test to cleanup eg singletons that were registered during the test execution </summary>
    public class CleanupHelper : IDisposable {

        private HashSet<Action> cleanupActions = new HashSet<Action>();

        public void Dispose() {
            if (cleanupActions.IsEmpty()) {
                Log.w("SingletonCleanup was setup but never filled with singletons before it was disposed");
            } else {
                foreach (var cleanupAction in cleanupActions) {
                    cleanupAction();
                }
            }
        }

        public void AddDisposable(IDisposable disposables) {
            AddCleanupAction(() => {
                if (disposables is IDisposableV2 d2) {
                    d2.DisposeV2();
                } else {
                    disposables.Dispose();
                }
            });
        }

        public void AddInjectorsCleanup(IEnumerable<Tuple<object, Type>> collectedInjectors) {
            foreach (var injector in collectedInjectors) {
                AddInjectorCleanup(injector.Item1, injector.Item2);
            }
        }

        public void AddInjectorCleanup<T>(object injector) { AddInjectorCleanup(injector, typeof(T)); }

        public void AddInjectorCleanup(object injector, Type type) {
            Action cleanUpAction = delegate { IoC.inject.UnregisterInjector(injector, type); };
            AddCleanupAction(cleanUpAction);
        }

        public void AddCleanupAction(Action cleanUpAction) { cleanupActions.Add(cleanUpAction); }

    }

}