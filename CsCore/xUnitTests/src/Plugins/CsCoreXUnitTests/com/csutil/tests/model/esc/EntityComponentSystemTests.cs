using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using com.csutil.model;
using com.csutil.model.ecs;
using Newtonsoft.Json;
using Xunit;

namespace com.csutil.tests.model.esc {

    public class EntityComponentSystemTests {

        public EntityComponentSystemTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsageOfTemplatesIO() {

            var rootDir = EnvironmentV2.instance.GetOrAddTempFolder("EntityComponentSystemTests_ExampleUsage1");
            var templatesDir = rootDir.GetChildDir("Templates");
            templatesDir.DeleteV2();
            templatesDir.CreateV2();

            var templates = new TemplatesIO<Entity>(templatesDir);

            var enemyTemplate = new Entity() {
                LocalPose = Matrix4x4.CreateTranslation(1, 2, 3),
                Components = CreateComponents(
                    new EnemyComponent() { Id = "c1", Health = 100, Mana = 10 }
                )
            };
            templates.SaveAsTemplate(enemyTemplate);

            // An instance that has a different health value than the template:
            Entity variant1 = templates.CreateVariantInstanceOf(enemyTemplate);
            (variant1.Components.Single().Value as EnemyComponent).Health = 200;
            templates.SaveAsTemplate(variant1); // Save it as a variant of the enemyTemplate

            // Create a variant2 of the variant1
            Entity variant2 = templates.CreateVariantInstanceOf(variant1);
            (variant2.Components.Single().Value as EnemyComponent).Mana = 20;
            templates.SaveAsTemplate(variant2);

            // Updating variant 1 should also update variant2:
            (variant1.Components.Single().Value as EnemyComponent).Health = 300;
            templates.SaveAsTemplate(variant1);
            variant2 = templates.LoadTemplateInstance(variant2.Id);
            Assert.Equal(300, (variant2.Components.Single().Value as EnemyComponent).Health);

            {
                // Another instance that is identical to the template:
                Entity instance3 = templates.CreateVariantInstanceOf(enemyTemplate);
                // instance3 is not saved as a variant 
                // Creating an instance of an instance is not allowed:
                Assert.Throws<InvalidOperationException>(() => templates.CreateVariantInstanceOf(instance3));
                // Instead the parent template should be used to create another instance:
                Entity instance4 = templates.CreateVariantInstanceOf(templates.LoadTemplateInstance(instance3.TemplateId));
                Assert.Equal(instance3.TemplateId, instance4.TemplateId);
                Assert.NotEqual(instance3.Id, instance4.Id);
            }

            var ecs2 = new TemplatesIO<Entity>(templatesDir);

            var ids = ecs2.GetAllEntityIds().ToList();
            Assert.Equal(3, ids.Count());

            Entity v1 = ecs2.LoadTemplateInstance(variant1.Id);
            var enemyComp1 = v1.Components.Single().Value as EnemyComponent;
            Assert.Equal(300, enemyComp1.Health);
            Assert.Equal(10, enemyComp1.Mana);

            // Alternatively to automatically lazy loading the templates can be loaded into memory all at once: 
            await ecs2.LoadAllTemplateFilesIntoMemory();

            Entity v2 = ecs2.LoadTemplateInstance(variant2.Id);
            var enemyComp2 = v2.Components.Single().Value as EnemyComponent;
            Assert.Equal(300, enemyComp2.Health);
            Assert.Equal(20, enemyComp2.Mana);

        }

        [Fact]
        public async Task ExampleUsageOfEcs() {

            var ecs = new EntityComponentSystem<Entity>(null);

            var entityGroup = ecs.Add(new Entity() {
                LocalPose = Matrix4x4.CreateRotationY(MathF.PI / 2) // 90 degree rotation around y axis
            });

            var e1 = entityGroup.AddChild(new Entity() {
                LocalPose = Matrix4x4.CreateRotationY(-MathF.PI / 2), // -90 degree rotation around y axis
            }, AddToChildrenListOfParent);

            var e2 = entityGroup.AddChild(new Entity() {
                LocalPose = Matrix4x4.CreateTranslation(1, 0, 0),
            }, AddToChildrenListOfParent);

            var children = entityGroup.GetChildren();
            Assert.Equal(2, children.Count());
            Assert.Same(e1, children.First());
            Assert.Same(e2, children.Last());
            Assert.Same(e1.GetParent(), entityGroup);
            Assert.Same(e2.GetParent(), entityGroup);

            { // Local and global poses can be accessed like this:
                var rot90Degree = Quaternion.CreateFromYawPitchRoll(MathF.PI / 2, 0, 0);
                Assert.Equal(rot90Degree, entityGroup.GlobalPose().rotation);
                Assert.Equal(rot90Degree, entityGroup.LocalPose().rotation);

                // e2 does not have a local rot so the global rot is the same as of the parent:
                Assert.Equal(rot90Degree, e2.GlobalPose().rotation);
                Assert.Equal(Quaternion.Identity, e2.LocalPose().rotation);

                // e1 has a local rotation that is opposite of the parent 90 degree, the 2 cancel each other out:
                Assert.Equal(Quaternion.Identity, e1.GlobalPose().rotation);
                var rotMinus90Degree = Quaternion.CreateFromYawPitchRoll(-MathF.PI / 2, 0, 0);
                Assert.Equal(rotMinus90Degree, e1.LocalPose().rotation);

                // e1 is in the center of the parent, its global pos isnt affected by the rotation of the parent:
                Assert.Equal(Vector3.Zero, e1.GlobalPose().position);
                Assert.Equal(Vector3.Zero, e1.LocalPose().position);

                // Due to the rotation of the parent the global position of e2 is now (0,0,1):
                Assert.Equal(new Vector3(1, 0, 0), e2.LocalPose().position);
                Assert_AlmostEqual(new Vector3(0, 0, -1), e2.GlobalPose().position);

                // The scales are all 1:
                Assert.Equal(Vector3.One, e1.GlobalPose().scale);
                Assert.Equal(Vector3.One, e1.LocalPose().scale);
            }

            Assert.Equal(3, ecs.AllEntities.Count);
            e1.RemoveFromParent(RemoveChildIdFromParent);
            // e1 is removed from its parent but still in the scene graph:
            Assert.Equal(3, ecs.AllEntities.Count);
            Assert.Same(e2, entityGroup.GetChildren().Single());
            Assert.Null(e1.GetParent());
            Assert.True(e1.Destroy(RemoveChildIdFromParent));
            Assert.False(e1.Destroy(RemoveChildIdFromParent));
            // e1 is now fully removed from the scene graph and destroyed:
            Assert.Equal(2, ecs.AllEntities.Count);

            Assert.False(e2.IsDestroyed());

            var e3 = e2.AddChild(new Entity(), AddToChildrenListOfParent);
            var e4 = e3.AddChild(new Entity(), AddToChildrenListOfParent);

            Assert.True(e2.Destroy(RemoveChildIdFromParent));
            Assert.Empty(entityGroup.GetChildren());

            Assert.True(e2.IsDestroyed());
            Assert.Equal(1, ecs.AllEntities.Count);

            // Since e3 and e4 are in the subtree of e2 they are also destroyed:
            Assert.True(e3.IsDestroyed());
            Assert.True(e4.IsDestroyed());

        }

        [Fact]
        public async Task TestEcsPoseMath() {

            /* A test that composes a complex nested scene graph and checks if the
             * global pose of the most inner entity is back at the origin (validated that
             * same result is achieved with Unity) */

            var ecs = new EntityComponentSystem<Entity>(null);

            var e1 = ecs.Add(new Entity() {
                LocalPose = Pose.NewMatrix(new Vector3(0, 1, 0))
            });

            var e2 = e1.AddChild(new Entity() {
                LocalPose = Pose.NewMatrix(new Vector3(0, 1, 0), 90)
            }, AddToChildrenListOfParent);

            var e3 = e2.AddChild(new Entity() {
                LocalPose = Pose.NewMatrix(new Vector3(0, 0, 2), 0, 2)
            }, AddToChildrenListOfParent);

            var e4 = e3.AddChild(new Entity() {
                LocalPose = Pose.NewMatrix(new Vector3(0, 0, -1), -90)
            }, AddToChildrenListOfParent);

            var e5 = e4.AddChild(new Entity() {
                LocalPose = Pose.NewMatrix(new Vector3(0, -1, 0), 0, 0.5f)
            }, AddToChildrenListOfParent);

            var pose = e5.GlobalPose();
            Assert.Equal(Quaternion.Identity, pose.rotation);
            Assert_AlmostEqual(Vector3.One, pose.scale);
            Assert.Equal(Vector3.Zero, pose.position);

        }

        /// <summary> Shows how to create a scene at runtime, persist it to disk and reload it </summary>
        [Fact]
        public async Task ExampleRuntimeSceneCreationPersistenceAndReloading() {

            // First the user creates a scene at runtime:
            var dir = EnvironmentV2.instance.GetNewInMemorySystem();
            {
                var ecs = new EntityComponentSystem<Entity>(new TemplatesIO<Entity>(dir));

                // He defines a few of the entities as templates and other as variants

                // Define a base enemy template with a sword:
                var baseEnemy = ecs.Add(new Entity() {
                    Name = "EnemyTemplate",
                    Components = CreateComponents(new EnemyComponent() { Health = 100, Mana = 0 })
                });
                baseEnemy.AddChild(new Entity() {
                    Name = "Sword",
                    Components = CreateComponents( new SwordComponent() { Damage = 10 } )
                }, AddToChildrenListOfParent);
                baseEnemy.SaveChanges();

                // Define a variant of the base enemy which is stronger and has a shield:
                var bossEnemy = baseEnemy.CreateVariant();
                bossEnemy.Data.Name = "BossEnemy";
                bossEnemy.GetComponent<EnemyComponent>().Health = 200;
                bossEnemy.AddChild(new Entity() {
                    Name = "Shield",
                    Components = CreateComponents( new ShieldComponent() { Defense = 10 } )
                }, AddToChildrenListOfParent);
                bossEnemy.SaveChanges();

                // Define a mage variant that has mana but no sword
                var mageEnemy = baseEnemy.CreateVariant();
                mageEnemy.Data.Name = "MageEnemy";
                mageEnemy.GetComponent<EnemyComponent>().Mana = 100;
                var sword = mageEnemy.GetChild("Sword");

                // Switching the parent of the sword from the mage to the boss enemy should fail
                Assert.Throws<InvalidOperationException>(() => bossEnemy.AddChild(sword, AddToChildrenListOfParent));
                // Instead the sword first needs to be removed and then added to the new parent:
                sword.RemoveFromParent(RemoveChildIdFromParent);
                bossEnemy.AddChild(sword, AddToChildrenListOfParent);

                bossEnemy.SaveChanges();
                mageEnemy.SaveChanges();

                // Updates to the prefabs also result in the variants being updated
                baseEnemy.GetComponent<EnemyComponent>().Health = 150;
                baseEnemy.SaveChanges();
                // The mage enemy health wasnt overwritten so with the template update it now has also 150 health:
                Assert.Equal(150, mageEnemy.GetComponent<EnemyComponent>().Health);

                // All created entities are added to the scene graph and persisted to disk
                var scene = ecs.Add(new Entity() { Name = "Scene" });
                var enemy1 = scene.AddChild(baseEnemy.CreateVariant(), AddToChildrenListOfParent);
                enemy1.Data.LocalPose = Pose.NewMatrix(new Vector3(1, 0, 0));
                var enemy2 = scene.AddChild(bossEnemy.CreateVariant(), AddToChildrenListOfParent);
                enemy2.Data.LocalPose = Pose.NewMatrix(new Vector3(0, 0, 1));
                var enemy3 = scene.AddChild(mageEnemy.CreateVariant(), AddToChildrenListOfParent);
                enemy3.Data.LocalPose = Pose.NewMatrix(new Vector3(-1, 0, 0));

                scene.SaveChanges();

                // Simulate the user closing the application and starting it again
                ecs.Dispose();
            }
            {
                var ecs = new EntityComponentSystem<Entity>(new TemplatesIO<Entity>(dir));
                Assert.Empty(ecs.AllEntities);
                await ecs.LoadSceneGraphFromDisk();
                Assert.Equal(9, dir.EnumerateFiles().Count());
                Assert.Equal(9, ecs.AllEntities.Count);
                // The user loads the scene from disk and can continue editing it

                var scene = ecs.FindEntitiesWithName("Scene").Single();

                Assert.Equal(3, scene.GetChildren().Count());
                var enemy1 = scene.GetChildren().ElementAt(0);
                Assert.Equal(new Vector3(1, 0, 0), enemy1.LocalPose().position);
                var enemy2 = scene.GetChildren().ElementAt(1);
                Assert.NotNull(enemy2.GetComponentInChildren<Entity, ShieldComponent>());
            }
        }
        
        private IReadOnlyDictionary<string, IComponentData> CreateComponents(IComponentData component) {
            component.GetId().ThrowErrorIfNullOrEmpty("component.GetId()");
            var dict = new Dictionary<string, IComponentData>();
            dict.Add(component.GetId(), component);
            return dict;
        }

        private void Assert_AlmostEqual(Vector3 a, Vector3 b, float allowedDelta = 0.000001f) {
            var length = (a - b).Length();
            Assert.True(length < allowedDelta, $"Expected {a} to be almost equal to {b} but the difference is {length}");
        }

        private static Entity AddToChildrenListOfParent(IEntity<Entity> parent, string addedChildId) {
            parent.Data.MutablehildrenIds.Add(addedChildId);
            return parent.Data;
        }

        private Entity RemoveChildIdFromParent(IEntity<Entity> parent, string childIdToRemove) {
            parent.Data.MutablehildrenIds.Remove(childIdToRemove);
            return parent.Data;
        }

        private class Entity : IEntityData {

            public string Id { get; set; } = "" + GuidV2.NewGuid();
            public string Name { get; set; }
            public string TemplateId { get; set; }
            public Matrix4x4? LocalPose { get; set; }
            public IReadOnlyDictionary<string, IComponentData> Components { get; set; }

            public IReadOnlyList<string> ChildrenIds => MutablehildrenIds;
            [JsonIgnore] // Dont include the children ids two times
            public List<string> MutablehildrenIds { get; } = new List<string>();

            public string GetId() { return Id; }

        }

        private class EnemyComponent : IComponentData {
            public string Id { get; set; } = "" + GuidV2.NewGuid();
            public int Mana { get; set; }
            public int Health;
            public string GetId() { return Id; }
        }

        private class SwordComponent : IComponentData {
            public string Id { get; set; } = "" + GuidV2.NewGuid();
            public int Damage { get; set; }
            public string GetId() { return Id; }
        }

        private class ShieldComponent : IComponentData {
            public string Id { get; set; } = "" + GuidV2.NewGuid();
            public int Defense { get; set; }
            public string GetId() { return Id; }
        }

    }

}