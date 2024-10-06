namespace com.csutil.model.ecs {
    
    /// <summary> Can be implemented by a component in an entity to get informed whenever the entity data is updated </summary>
    public interface IParentEntityUpdateListener<T> where T : IEntityData {

        /// <summary> Informs the IEntity if its content (the entity data) was updated. Will not fire for the initial creation of
        /// the entity or the removal/destruction of the entity in the ecs </summary>
        void OnParentEntityUpdate(T oldEntityState, IEntity<T> newEntityState);

    }
    
    /// <summary> Can be implemented by an ECS component to get informed when the parent entity of the component becomes part of the ECS system </summary>
    public interface IEntityInEcsListener<T> where T : IEntityData {

        /// <summary> Will be triggered when the parent entity of the component becomes part of the
        /// ECS system (because its added to the ECS or because the ECS is loaded/restored) </summary>
        void OnEntityNowInEcs(IEntity<T> entity);

    }
    
}