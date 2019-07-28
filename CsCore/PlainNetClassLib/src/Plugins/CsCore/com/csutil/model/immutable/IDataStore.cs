using System;

namespace com.csutil.model.immutable {

    public interface IDataStore<T> {

        Action onStateChanged { get; set; }

        T GetState();

        object Dispatch(object action);

    }

}