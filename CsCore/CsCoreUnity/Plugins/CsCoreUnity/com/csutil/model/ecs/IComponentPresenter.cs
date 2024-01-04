namespace com.csutil.model.ecs {
    
    public interface IComponentPresenter<T> : IDisposableV2 where T : IEntityData {
        string ComponentId { get; set; }
        void OnUpdateUnityComponent(IEntity<T> iEntity, IComponentData oldState, IComponentData updatedState);
    }
    
}