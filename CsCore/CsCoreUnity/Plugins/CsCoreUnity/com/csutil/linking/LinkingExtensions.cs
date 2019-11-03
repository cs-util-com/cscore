using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public static class LinkingExtensions {

        public static Dictionary<string, Link> GetLinkMap(this GameObject self, bool includeInactive = true) {
            var linkArray = self.GetComponentsInChildren<Link>(includeInactive);
            var linkMap = new Dictionary<string, Link>();
            foreach (var link in linkArray) {
                if (linkMap.ContainsKey(link.id)) { throw Log.e("Multiple links with same id=" + link.id, link.gameObject); }
                linkMap.Add(link.id, link);
                //link.NowLoadedIntoLinkMap(links); // TODO?
            }
            EventBus.instance.Publish(LinkingEvents.LINK_MAP_CREATED, self, linkMap);
            return linkMap;
        }

        public static T Get<T>(this Dictionary<string, Link> self, string id) {
            try {
                if (typeof(T) == typeof(GameObject)) { return (T)(object)self[id].gameObject; }
                return self[id].GetComponent<T>();
            }
            catch (KeyNotFoundException) { throw new KeyNotFoundException("No Link found with id=" + id); }
        }

    }

}
