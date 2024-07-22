using System;
using UnityEngine;

namespace com.csutil.model.ecs {

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