using System;

namespace com.csutil.model.immutable {

    public interface IHasReducer<T> {

        /// <summary>
        /// The reducer can be called manually if an immutable model is used to simulate the
        /// effects of an action on the state without actually changing the state in the
        /// store yet. Changing the action state can only be done (if the model is
        /// immutable) by using the Dispatch method.
        /// </summary>
        StateReducer<T> reducer { get; }

    }

    public interface IDataStoreDispatcher {
        
        /// <summary> Dispatches an action to the store so that afterwards GetState()
        /// can be called to get the new state. </summary>
        /// <param name="action"> The action to dispatch to the store </param>
        /// <returns> the dispatcher decides what object is returned, typically its the
        /// action itself </returns>
        object Dispatch(object action);
        
    }
    
    public interface IDataStore<T> : IDataStoreDispatcher, IHasReducer<T> {

        /// <summary> The onStateChanged callback is called after the state of the
        /// store was changed </summary>
        Action onStateChanged { get; set; }

        /// <summary> Returns the current state of the store </summary>
        T GetState();

    }

}