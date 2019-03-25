using com.csutil.ui;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests.ui {

    class ViewStackTests {

        [UnityTest]
        public IEnumerator ExampleUsage1() {

            var viewStackGo = new GameObject();
            viewStackGo.AddComponent<ViewStack>();

            var view1 = viewStackGo.AddChild(new GameObject("View 1"));

            var view2 = view1.GetViewStack().ShowView(view1, new GameObject("View 2"));
            Assert.IsFalse(view1.activeInHierarchy);

            Assert.IsTrue(view2.GetViewStack().SwitchBackToLastView(view2));
            Assert.IsTrue(view2.IsDestroyed());
            Assert.IsTrue(view1.activeInHierarchy);

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestViewStack1() {

            var viewStackGo = new GameObject();
            viewStackGo.AddComponent<ViewStack>();

            var view1 = viewStackGo.AddChild(new GameObject("View 1"));
            Assert.AreEqual(viewStackGo.GetComponent<ViewStack>(), view1.GetViewStack());

            var view2 = view1.GetViewStack().ShowView(view1, new GameObject("View 2"));
            Assert.IsFalse(view1.activeInHierarchy);

            var view3 = view1.GetViewStack().ShowView(view1, new GameObject("View 3"), hideCurrentView: false);
            Assert.IsTrue(view2.activeInHierarchy);

            // Now test SwitchBackToLastView plus the events the view stack sends out:
            EventBus.instance.Subscribe(this, UiEvents.SWITCH_BACK_TO_LAST_VIEW, (GameObject lastView) => {
                Assert.AreSame(view2, lastView);
                EventBus.instance.Unsubscribe(this, UiEvents.SWITCH_BACK_TO_LAST_VIEW);
            });
            Assert.IsTrue(view3.GetViewStack().SwitchBackToLastView(view3));
            Assert.IsTrue(view3.IsDestroyed());
            Assert.IsTrue(view2.activeInHierarchy);

            Assert.IsTrue(view2.GetViewStack().SwitchBackToLastView(view2));
            Assert.IsTrue(view2.IsDestroyed());
            Assert.IsTrue(view1.activeInHierarchy);

            Assert.IsFalse(view1.GetViewStack().SwitchBackToLastView(view1));
            Assert.IsTrue(view1.activeInHierarchy); // view 1 still active because its the last one

            yield return null;
        }
    }
}
