using UnityEngine;

namespace com.csutil.model.ecs {

    /// <summary> This presenter/view is added for every <see cref="IEntityData"/> to provide access to
    /// the model from all other components/views added for the component data of that entity. </summary>
    public class EntityView : MonoBehaviour {

        public IEntityData IEntity { get; private set; }

        public void Init<T>(IEntity<T> iEntity) where T : IEntityData {
            if (this.IEntity != null) {
                throw Log.e("EntityView can only be initialized once", gameObject);
            }
            this.IEntity = iEntity;
        }

#if UNITY_EDITOR
        public string debugJsonIEntity;

        // In editor update the debugJsonIEntity when the gameobject is selected:
        private void OnValidate() {
            if (IEntity != null) { debugJsonIEntity = IEntity.ToExtendedEntityString(); }
        }
#endif

    }

}