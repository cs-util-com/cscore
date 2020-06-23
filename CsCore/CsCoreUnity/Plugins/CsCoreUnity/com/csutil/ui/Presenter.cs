using com.csutil.model.immutable;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil {

    // https://en.wikipedia.org/wiki/Model–view–presenter
    public interface Presenter<T> {

        /// <summary> The target view to load the model into </summary>
        GameObject targetView { get; set; }

        /// <summary> Called to load the model into the targetView </summary>
        Task OnLoad(T model);

    }

    public static class PresenterExtensions {

        /// <summary> Connects a model with a view </summary>
        /// <returns> A task that can be awaited on, that returns the fully setup presenter </returns>
        public static async Task<T> LoadModelIntoView<T>(this Presenter<T> self, T model) {
            AssertV2.IsNotNull(self.targetView, "presenter.targetView");
            AssertV2.IsNotNull(model, $"model (type={typeof(T)})");

            var presenterName = self.GetType().Name;
            var modelName = model.GetType().Name;
            var viewName = self.targetView.name;
            var name = $"{presenterName}({modelName})--{viewName}";

#if UNITY_EDITOR // In the Unity editor always try to use the Visual Assert system by default:
            await AssertVisually.AssertNoVisualChange(name);
#endif
            EventBus.instance.Publish(EventConsts.catPresenter + EventConsts.LOAD_START, name, self, model);
            await self.OnLoad(model);
            EventBus.instance.Publish(EventConsts.catPresenter + EventConsts.LOAD_DONE, name, self, model);
            return model;
        }

        /// <summary> This will observe a specific subpart of the state that should be shown in the target presenter, and it will automatically
        /// call the OnLoad method of the presenter when the target subpart of the state changes. This should be taken into account by
        /// the presenter that OnLoad will happen multiple times and can happen at any time. </summary>
        /// <typeparam name="Model">The part of the state that will be shown in the presenter</typeparam>
        /// <param name="store">The target store that will be observerd</param>
        /// <param name="getSubState"> Select the substate part that is relevant for the presenter and return it in the fuction</param>
        /// <returns> A task that contains the first user model change made by the target presenter, so this can be awaited for presenters that 
        /// await a final save button click and are closed afterwards. If the task is awaited for a presenter that stays open and does not have
        /// a linear flow like a form that closes itself in the end then the task cant be properly used and should not be awaited!</returns>
        public static Task<Model> ListenToStoreUpdates<T, Model>(this Presenter<Model> self, IDataStore<T> store, Func<T, Model> getSubState) {
            TaskCompletionSource<Model> tcs = new TaskCompletionSource<Model>();
            store.AddAsyncStateChangeListener(getSubState, async (newValue) => {
                tcs.TrySetResult(await self.LoadModelIntoView(newValue));
            });
            return tcs.Task;
        }

    }

}