namespace com.csutil.model.ecs {

    /// <summary> Can be implemented by a component presenter to handle the OnDestroy step of the component,
    /// e.g. not destroying the presenter but resetting it instead </summary>
    public interface IEcsComponentDestroyHandler {

        /// <summary> Will be component of the presenter was destroyed </summary>
        /// <returns> If false is returned the component presenter will not be destroyed by the ECS presenter system </returns>
        bool OnDestroyRequest();
    }

}