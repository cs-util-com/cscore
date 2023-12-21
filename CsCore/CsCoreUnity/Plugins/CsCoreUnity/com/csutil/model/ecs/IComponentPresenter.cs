namespace com.csutil.model.ecs {
    
    public interface IComponentPresenter<T> : IDisposableV2 where T : IEntityData {
        void OnUpdateUnityComponent(IEntity<T> iEntity, IComponentData oldState, IComponentData updatedState);
    }
    
}