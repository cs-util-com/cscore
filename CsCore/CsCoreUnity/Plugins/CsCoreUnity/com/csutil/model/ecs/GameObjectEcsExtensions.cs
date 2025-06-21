using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace com.csutil.model.ecs {

    public static class GameObjectEcsExtensions {

        public static T GetComponentInOwnEcsPresenterChildren<T>(this GameObject self, IEntityData iEntity, bool throwExceptionIfNull = false, bool includeInactive = true) {
            var result = self.GetComponentsInOwnEcsPresenterChildren<T>(iEntity, throwExceptionIfNull).FirstOrDefault();
            if (throwExceptionIfNull && result == null) {
                throw Log.e("No child found with component of type " + typeof(T), self);
            }
            return result;
        }

        public static IEnumerable<T> GetComponentsInOwnEcsPresenterChildren<T>(this GameObject self, IEntityData iEntity, bool throwExceptionIfEmpty = false, bool includeInactive = true) {
            var ownEntityId = iEntity.Id;
            var presentersOfOwnEntity = self.GetBreadthFirstChildrenTree(includeInactive).Where(go => ownEntityId == go.GetComponentInParents<EntityView>().IEntity.Id);
            var componentsInThesePresenters = presentersOfOwnEntity.SelectMany(ev => ev.GetComponents<T>());
            if (throwExceptionIfEmpty && componentsInThesePresenters.IsEmpty()) {
                throw Log.e("No child found with component of type " + typeof(T), self);
            }
            return componentsInThesePresenters;
        }

    }

}