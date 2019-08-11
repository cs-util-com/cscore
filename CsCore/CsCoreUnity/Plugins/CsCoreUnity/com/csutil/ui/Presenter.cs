using com.csutil.model.immutable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.ui {

    // https://en.wikipedia.org/wiki/Model–view–presenter
    public interface Presenter<T> {

        GameObject targetView { get; set; }

        Task OnLoad(T model);

    }

    public static class PresenterExtensions {

        /// <summary> Connects a model with a view </summary>
        /// <returns> A task that can be awaited on, that returns the fully setup presenter </returns>
        public static async Task<Presenter<T>> LoadModelIntoView<T>(this Presenter<T> self, T model) {
            AssertV2.IsNotNull(self.targetView, "presenter.targetView");
            await self.OnLoad(model);
            return self;
        }

        public static void ListenToStoreUpdates<T, S>(this Presenter<S> self, IDataStore<T> store, Func<T, S> getSubState) {
            store.AddStateChangeListener(getSubState, (newValue) => { return self.LoadModelIntoView(newValue); });
        }

    }

}