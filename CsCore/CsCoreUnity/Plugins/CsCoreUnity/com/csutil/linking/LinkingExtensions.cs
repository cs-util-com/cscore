using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace com.csutil {

    public static class LinkingExtensions {

        public static Dictionary<string, Link> GetLinkMap(this GameObject self, bool includeInactive = true) {
            var linkArray = self.GetComponentsInChildren<Link>(includeInactive);
            var linkMap = new Dictionary<string, Link>();
            foreach (var link in linkArray) {
                if (link.id.IsNullOrEmpty()) {
                    throw Log.e("Link with empty id string found: " + link.gameObject, link.gameObject);
                }
                if (linkMap.ContainsKey(link.id)) {
                    var e = Log.e("Multiple links with same id=" + link.id, link.gameObject);
                    var obj1 = linkMap[link.id].gameObject;
                    var obj2 = link.gameObject;
                    Log.e("Obj 1:" + obj1, obj1);
                    Log.e("Obj 2:" + obj2, obj2);
                    throw e;
                }
                linkMap.Add(link.id, link);
                //link.NowLoadedIntoLinkMap(links); // TODO?
            }
            EventBus.instance.Publish(EventConsts.catLinked, self, linkMap);
            return linkMap;
        }

        public static void ActivateLinkMapTracking(this IAppFlow self) {
            EventBus.instance.Subscribe(self, EventConsts.catLinked, (GameObject target, Dictionary<string, Link> links) => {
                self.TrackEvent(EventConsts.catLinked, $"Collect_{links.Count}_Links_" + target.name, target, links);
            });
        }

        public static T Get<T>(this Dictionary<string, Link> self, string id) {
            try { return Get<T>(self[id]); } catch (KeyNotFoundException) { throw new KeyNotFoundException("No Link found with id=" + id); }
        }

        public static T Get<T>(this Link self) {
            if (typeof(T) == typeof(GameObject)) { return (T)(object)self.gameObject; }
            var comp = self.GetComponentV2<T>();
            if (comp == null) { CheckTmpAlternativePossible<T>(self); }
            return comp == null ? (T)(object)null : comp;
        }
        
        /// <summary> This method checks if the UI already switched to using TMP while the code still uses the
        /// old UI components and throws in improved exception instead of just returning null </summary>
        private static void CheckTmpAlternativePossible<T>(Link self) {
            if (typeof(T) == typeof(UnityEngine.UI.Text)) {
                if (self.HasComponent<TMP_Text>(out var tmpText)) {
                    throw Log.e("Found TMP_Text instead of UnityEngine.UI.Text for " + self.id, tmpText);
                }
            } else if (typeof(T) == typeof(UnityEngine.UI.InputField)) {
                if (self.HasComponent<TMP_InputField>(out var tmpInputField)) {
                    throw Log.e("Found TMP_InputField instead of UnityEngine.UI.InputField for " + self.id, tmpInputField);
                }
            }
        }
        
    }

}
