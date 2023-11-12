namespace com.csutil {

    /// <summary> A UI entity such a presenter or view can implement this interface to provide access to
    /// the actions that can be performed on the model they are showing and interacting with. </summary>
    /// <typeparam name="M"> The model class the entity is interacting with </typeparam>
    /// <typeparam name="A"> The model-actions class the entity is interacting with </typeparam>
    public interface IHasActions<M, A> where A : IModelActions<M> {
        /// <summary> The actions that can be performed on the model </summary>
        A actions { get; }
    }

    /// <summary> Performing actions/mutations on (parts of) the model can be written in dedicated IModelActions classes.
    /// Actions can be both mutations of the model but also events like button clicks or other user interactions.
    /// The IModelActions interface is used to decouple the presenter from the view and the model.
    /// The presenter can be reused with different views and models as long as they implement the IModelActions interface. </summary>
    public interface IModelActions<M> {
        /// <summary> The model instance (that is currently loaded into the view) where the triggered actions can be performed on </summary>
        M Model { set; }
    }

}