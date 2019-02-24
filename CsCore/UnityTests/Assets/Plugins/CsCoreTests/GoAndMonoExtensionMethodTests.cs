using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil.tests.extensions {

    public class GoAndMonoExtensionMethodTests {

        [Test]
        public void ExampleUsage1() {
            GameObject myGo = new GameObject();
            // Adding children GameObjects via AddChild:
            GameObject myChildGo = myGo.AddChild(new GameObject());
            // Getting the parent of the child via GetParent:
            Assert.AreSame(myGo, myChildGo.GetParent());

            // Lazy-initialization of the GameObject in case it does not yet exist:
            GameObject child1 = myGo.GetOrAddChild("Child 1");
            // Lazy-initialization of the Mono in case it does not yet exist:
            MyExampleMono1 myMono1 = child1.GetOrAddComponent<MyExampleMono1>();
            // Calling the 2 methods again results always in the same mono:
            var myMono1_ref2 = myGo.GetOrAddChild("Child 1").GetOrAddComponent<MyExampleMono1>();
            Assert.AreSame(myMono1, myMono1_ref2);

            myGo.Destroy(); // Destroy the gameobject
            Assert.IsTrue(myGo.IsDestroyed()); // Check if it was destroyed
        }

        [Test]
        public void Test1() {
            GameObject go1 = new GameObject();
            GameObject go1_child1 = go1.AddChild(new GameObject());
            Assert.AreSame(go1, go1_child1.GetParent());

            GameObject go1_child2 = go1.GetOrAddChild("GO1 Child2");
            Assert.AreSame(go1_child2, go1.GetOrAddChild("GO1 Child2"));
            Assert.IsNull(go1_child2.GetComponent<MyExampleMono1>());
            MyExampleMono1 mono1 = go1_child2.GetOrAddComponent<MyExampleMono1>();
            Assert.NotNull(mono1);
            Assert.AreSame(mono1, go1_child2.GetOrAddComponent<MyExampleMono1>());
        }

        [Test]
        public void TestDestroy() {
            GameObject goToDestroy = null;
            Assert.IsFalse(goToDestroy.Destroy());
            Assert.IsFalse(goToDestroy.IsDestroyed());
            goToDestroy = new GameObject();
            Assert.IsFalse(goToDestroy.IsDestroyed());
            Assert.IsTrue(goToDestroy.Destroy());
            Assert.IsTrue(goToDestroy.IsDestroyed());
        }

    }

}
