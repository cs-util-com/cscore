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

        private const float radToDegree = 180f / MathF.PI;
        private const float degreeToRad = MathF.PI / 180f;

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
            await templates.SaveChanges(enemyTemplate);

            // An instance that has a different health value than the template:
            Entity variant1 = templates.CreateVariantInstanceOf(enemyTemplate, NewIdDict(enemyTemplate));
            (variant1.Components.Single().Value as EnemyComponent).Health = 200;
            await templates.SaveChanges(variant1); // Save it as a variant of the enemyTemplate

            // Create a variant2 of the variant1
            Entity variant2 = templates.CreateVariantInstanceOf(variant1, NewIdDict(variant1));
            (variant2.Components.Single().Value as EnemyComponent).Mana = 20;
            await templates.SaveChanges(variant2);

            // Updating variant 1 should also update variant2:
            (variant1.Components.Single().Value as EnemyComponent).Health = 300;
            await templates.SaveChanges(variant1);
            variant2 = templates.ComposeEntityInstance(variant2.Id);
            Assert.Equal(300, (variant2.Components.Single().Value as EnemyComponent).Health);

            {
                // Another instance that is identical to the template:
                Entity instance3 = templates.CreateVariantInstanceOf(enemyTemplate, NewIdDict(enemyTemplate));
                // instance3 is not saved as a variant 
                // Creating an instance of an instance is not allowed:
                Assert.Throws<InvalidOperationException>(() => templates.CreateVariantInstanceOf(instance3, NewIdDict(instance3)));
                // Instead the parent template should be used to create another instance:
                var template = templates.ComposeEntityInstance(instance3.TemplateId);
                Entity instance4 = templates.CreateVariantInstanceOf(template, NewIdDict(template));
                Assert.Equal(instance3.TemplateId, instance4.TemplateId);
                Assert.NotEqual(instance3.Id, instance4.Id);
            }

            var ecs2 = new TemplatesIO<Entity>(templatesDir);

            var ids = ecs2.GetAllEntityIds().ToList();
            Assert.Equal(3, ids.Count());

            Entity v1 = ecs2.ComposeEntityInstance(variant1.Id);
            var enemyComp1 = v1.Components.Single().Value as EnemyComponent;
            Assert.Equal(300, enemyComp1.Health);
            Assert.Equal(10, enemyComp1.Mana);

            // Alternatively to automatically lazy loading the templates can be loaded into memory all at once: 
            await ecs2.LoadAllTemplateFilesIntoMemory();

            Entity v2 = ecs2.ComposeEntityInstance(variant2.Id);
            var enemyComp2 = v2.Components.Single().Value as EnemyComponent;
            Assert.Equal(300, enemyComp2.Health);
            Assert.Equal(20, enemyComp2.Mana);

        }

        private Dictionary<string, string> NewIdDict(Entity original) {
            return new Dictionary<string, string>() { { original.Id, "" + GuidV2.NewGuid() } };
        }

        [Fact]
        public async Task ExampleUsageOfEcs() {

            var entityDir = EnvironmentV2.instance.GetNewInMemorySystem();
            var templatesIo = new TemplatesIO<Entity>(entityDir);
            var ecs = new EntityComponentSystem<Entity>(templatesIo, isModelImmutable: false);

            var entityGroup = ecs.Add(new Entity() {
                LocalPose = Matrix4x4.CreateRotationY(MathF.PI / 2) // 90 degree rotation around y axis
            });

            var e1 = entityGroup.AddChild(new Entity() {
                LocalPose = Matrix4x4.CreateRotationY(-MathF.PI / 2), // -90 degree rotation around y axis
            });

            var e2 = entityGroup.AddChild(new Entity() {
                LocalPose = Matrix4x4.CreateTranslation(1, 0, 0),
            });

            var children = entityGroup.GetChildren();
            Assert.Equal(2, children.Count());
            Assert.Same(e1, children.First());
            Assert.Same(e2, children.Last());
            Assert.Same(e1.GetParent(), entityGroup);
            Assert.Same(e2.GetParent(), entityGroup);

            { // Local and global poses can be accessed like this:
                var rot90Degree = Quaternion.CreateFromYawPitchRoll(90 * degreeToRad, 0, 0);
                Assert.Equal(rot90Degree, entityGroup.GlobalPose().rotation);
                Assert.Equal(rot90Degree, entityGroup.LocalPose().rotation);

                // e2 does not have a local rot so the global rot is the same as of the parent:
                Assert.Equal(rot90Degree, e2.GlobalPose().rotation);
                Assert.Equal(Quaternion.Identity, e2.LocalPose().rotation);

                // e1 has a local rotation that is opposite of the parent 90 degree, the 2 cancel each other out:
                Assert.Equal(Quaternion.Identity, e1.GlobalPose().rotation);
                var rotMinus90Degree = Quaternion.CreateFromYawPitchRoll(-90 * degreeToRad, 0, 0);
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

            Assert.Equal(3, ecs.Entities.Count);
            e1.RemoveFromParent();
            // e1 is removed from its parent but still in the scene graph:
            Assert.Equal(3, ecs.Entities.Count);
            Assert.Same(e2, entityGroup.GetChildren().Single());
            Assert.Null(e1.GetParent());
            Assert.True(await e1.Destroy());
            Assert.False(await e1.Destroy());
            // e1 is now fully removed from the scene graph and destroyed:
            Assert.Equal(2, ecs.Entities.Count);

            Assert.False(e2.IsDestroyed());

            var e3 = e2.AddChild(new Entity());
            var e4 = e3.AddChild(new Entity());

            Assert.True(await e2.Destroy());
            Assert.Empty(entityGroup.GetChildren());

            Assert.True(e2.IsDestroyed());
            Assert.Equal(1, ecs.Entities.Count);

            // Since e3 and e4 are in the subtree of e2 they are also destroyed:
            Assert.True(e3.IsDestroyed());
            Assert.True(e4.IsDestroyed());

        }

        [Fact]
        public async Task TestEcsEntityNesting() {

            /* A test that composes a complex nested scene graph and checks if attributes like the
             * global pose of the most inner entity is back at the origin (validated that
             * same result is achieved with Unity) */

            var entityDir = EnvironmentV2.instance.GetNewInMemorySystem();
            var templatesIo = new TemplatesIO<Entity>(entityDir);
            var ecs = new EntityComponentSystem<Entity>(templatesIo, isModelImmutable: false);

            var e1 = ecs.Add(new Entity() {
                LocalPose = Pose3d.NewMatrix(new Vector3(0, 1, 0))
            });

            var e2 = e1.AddChild(new Entity() {
                LocalPose = Pose3d.NewMatrix(new Vector3(0, 1, 0), 90)
            });

            var e3 = e2.AddChild(new Entity() {
                LocalPose = Pose3d.NewMatrix(new Vector3(0, 0, 2), 0, 2)
            });

            var e4 = e3.AddChild(new Entity() {
                LocalPose = Pose3d.NewMatrix(new Vector3(0, 0, -1), -90)
            });

            var e5 = e4.AddChild(new Entity() {
                LocalPose = Pose3d.NewMatrix(new Vector3(0, -1, 0), 0, 0.5f)
            });

            var pose = e5.GlobalPose();
            Assert.Equal(Quaternion.Identity, pose.rotation);
            Assert_AlmostEqual(Vector3.One, pose.scale);
            Assert.Equal(Vector3.Zero, pose.position);

            Assert.True(e5.IsActiveSelf());
            Assert.True(e5.IsActiveInHierarchy());
            e2.SetActiveSelf(false);
            Assert.False(e2.IsActiveSelf());
            Assert.False(e5.IsActiveInHierarchy());
            Assert.True(e5.IsActiveSelf());
            e5.SetActiveSelf(false);
            Assert.False(e5.IsActiveSelf());
            e2.SetActiveSelf(true);
            Assert.False(e5.IsActiveInHierarchy());
            e5.SetActiveSelf(true);
            Assert.True(e5.IsActiveInHierarchy());

        }

        [Fact]
        public void TestPoseOperators() {

            var poseTranslation = new Vector3(5, 10, 15);
            Matrix4x4 matrix = Pose3d.NewMatrix(poseTranslation, 180);
            var pose = matrix.ToPose();
            Assert_AlmostEqual(matrix, pose.ToMatrix4x4());
            Assert.Equal(poseTranslation, pose.position);
            Assert_AlmostEqual(Quaternion.CreateFromYawPitchRoll(180 * degreeToRad, 0, 0), pose.rotation);
            Assert_AlmostEqual(Vector3.One, pose.scale);

            var vecX_1_0_0 = Vector3.UnitX;

            // Counterclockwise rotation around the y/up axis:
            var rot90Degree = Quaternion.CreateFromYawPitchRoll(90 * degreeToRad, 0, 0);
            // The x axis is now pointing to the negative z axis:
            Assert_AlmostEqual(-Vector3.UnitZ, rot90Degree.ToMatrix4X4().Transform(vecX_1_0_0));

            // The pose has a position and rotation set which means (1,0,0) will end up rotated by 180 degree: 
            Assert.Equal(-Vector3.UnitX + poseTranslation, pose.ToMatrix4x4().Transform(vecX_1_0_0));

            { // To make it easier to work with poses they can be modified with operators:
                // Change the position of the pose:
                pose += vecX_1_0_0;
                Assert.Equal(vecX_1_0_0 + poseTranslation, pose.position);
                // Change the scale of the pose:
                pose *= 2;
                Assert.Equal(new Vector3(2, 2, 2), pose.scale);
                // Add a 90 degree rotation to the pose:
                pose = rot90Degree * pose;
                Assert_AlmostEqual(Quaternion.CreateFromYawPitchRoll((90 + 180) * degreeToRad, 0, 0), pose.rotation);
                Assert.Equal(new Vector3(2, 2, 2), pose.scale);
                Assert.Equal(vecX_1_0_0 + poseTranslation, pose.position);
            }

        }

        /// <summary> Shows how to create a scene at runtime, persist it to disk and reload it </summary>
        [Fact]
        public async Task ExampleRuntimeSceneCreationPersistenceAndReloading() {

            // First the user creates a scene at runtime:
            var dir = EnvironmentV2.instance.GetNewInMemorySystem();
            {
                var templatesIo = new TemplatesIO<Entity>(dir);
                var ecs = new EntityComponentSystem<Entity>(templatesIo, isModelImmutable: false);

                // He defines a few of the entities as templates and other as variants

                {
                    var entity1 = ecs.Add(new Entity() { Name = "Entity1" });
                    var entity11 = entity1.AddChild(new Entity() { Name = "Entity2" });
                    await entity1.SaveChanges();

                    var variant1 = entity1.CreateVariant();
                    Assert.NotEqual(variant1.Id, entity1.Id);
                    // The ids of the children are different:
                    Assert.NotEqual(entity1.ChildrenIds.Single(), variant1.ChildrenIds.Single());
                    Assert.NotSame(entity1.GetChildren().Single(), variant1.GetChildren().Single());

                    var variant11 = entity11.CreateVariant();
                    Assert.NotEqual(entity11.Id, variant11.Id);
                    // The variant is not attached to the same parent
                    Assert.NotEqual(entity11.ParentId, variant11.ParentId);
                    Assert.Null(variant11.ParentId);
                    // But the variant can be found in the root level of the ecs:
                    Assert.Same(variant11, ecs.GetEntity(variant11.Id));
                    // That means that the first entity also still does only have 1 child:
                    Assert.Single(entity1.GetChildren());
                    Assert.Single(entity1.ChildrenIds);
                }

                // Define a base enemy template with a sword:
                var baseEnemy = ecs.Add(new Entity() {
                    Name = "EnemyTemplate",
                    Components = CreateComponents(new EnemyComponent() { Health = 100, Mana = 0 })
                });
                baseEnemy.AddChild(new Entity() {
                    Name = "Sword",
                    Components = CreateComponents(new SwordComponent() { Damage = 10 })
                });
                await baseEnemy.SaveChanges();

                // Accessing components and children entities: 
                Assert.NotNull(baseEnemy.GetComponent<EnemyComponent>());
                Assert.Null(baseEnemy.GetComponent<SwordComponent>());
                Assert.NotNull(baseEnemy.GetChildren().Single().GetComponent<SwordComponent>());

                // Define a variant of the base enemy which is stronger and has a shield:
                var bossEnemy = baseEnemy.CreateVariant();
                bossEnemy.Data.Name = "BossEnemy";
                bossEnemy.GetComponent<EnemyComponent>().Health = 200;
                bossEnemy.AddChild(new Entity() {
                    Name = "Shield",
                    Components = CreateComponents(new ShieldComponent() { Defense = 10 })
                });
                await bossEnemy.SaveChanges();

                // Define a mage variant that has mana but no sword
                var mageEnemy = baseEnemy.CreateVariant();
                mageEnemy.Data.Name = "MageEnemy";
                mageEnemy.GetComponent<EnemyComponent>().Mana = 100;
                await mageEnemy.SaveChanges();

                Assert.NotSame(mageEnemy, baseEnemy);
                Assert.NotSame(mageEnemy.Data, baseEnemy.Data);

                var sword = mageEnemy.GetChild("Sword");
                Assert.NotEqual(sword, baseEnemy.GetChild("Sword"));
                Assert.True(sword.IsVariant());

                // Switching the parent of the sword from the mage to the boss enemy should fail
                Assert.Throws<InvalidOperationException>(() => bossEnemy.AddChild(sword));

                // Instead the sword first needs to be removed and then added to the new parent:
                Assert.Equal(sword.Id, mageEnemy.ChildrenIds.Single());
                sword.RemoveFromParent();
                Assert.Empty(mageEnemy.ChildrenIds);

                // Now that the sword is removed from the mage it can be added to the boss enemy: 
                bossEnemy.AddChild(sword);

                await bossEnemy.SaveChanges();
                await mageEnemy.SaveChanges();

                // The sword is still a variant of the sword in the EnemyTemplate
                Assert.True(sword.IsVariant());
                // The BossEnemy should now have 3 children (2 swords and 1 shield):
                Assert.Equal(3, bossEnemy.GetChildren().Count());

                // Updates to the prefabs also result in the variants being updated
                baseEnemy.GetComponent<EnemyComponent>().Health = 150;
                await baseEnemy.SaveChanges();

                // The mage enemy health wasnt modified so with the template update it now has also 150 health:
                Assert.Equal(150, mageEnemy.GetComponent<EnemyComponent>().Health);
                // The boss enemy was modified so it still has 200 health:
                Assert.Equal(200, bossEnemy.GetComponent<EnemyComponent>().Health);

                // If a change to a variant is done but not persisted via save changes and then a parent template of
                // that variant is changed the variant is reconstructed from the latest stored state and with that the
                // unsaved changes to the variant are lost:
                bossEnemy.GetComponent<EnemyComponent>().Health = 300;
                bossEnemy.GetComponent<EnemyComponent>().Mana = 50;
                // These boss enemy changes are NOT persisted (no call to SaveChanges) so they are lost once
                // baseEnemy saves its changes (and with that also updates all direct and indirect variants):
                await baseEnemy.SaveChanges();
                // The health is back to the 200 value as it was before and the mana is back to 0:
                Assert.Equal(200, bossEnemy.GetComponent<EnemyComponent>().Health);
                Assert.Equal(0, bossEnemy.GetComponent<EnemyComponent>().Mana);

                await bossEnemy.SaveChanges();
                await mageEnemy.SaveChanges();

                // All created entities are added to the scene graph and persisted to disk
                var scene = ecs.Add(new Entity() { Name = "Scene" });
                var enemy1 = scene.AddChild(baseEnemy.CreateVariant());
                enemy1.Data.LocalPose = Pose3d.NewMatrix(new Vector3(1, 0, 0));
                var enemy2 = scene.AddChild(bossEnemy.CreateVariant());
                enemy2.Data.LocalPose = Pose3d.NewMatrix(new Vector3(0, 0, 1));
                var enemy3 = scene.AddChild(mageEnemy.CreateVariant());
                enemy3.Data.LocalPose = Pose3d.NewMatrix(new Vector3(-1, 0, 0));

                await scene.SaveChanges();

                // Simulate the user closing the application and starting it again
                ecs.Dispose();
                // TODO
            }
            {
                Assert.NotEmpty(dir.EnumerateEntries());
                var templatesIo = new TemplatesIO<Entity>(dir);
                var ecs = new EntityComponentSystem<Entity>(templatesIo, isModelImmutable: false);
                Assert.Empty(ecs.Entities);
                await ecs.LoadSceneGraphFromDisk();
                Assert.Equal(17, dir.EnumerateFiles().Count());
                Assert.Equal(17, ecs.Entities.Count);
                // The user loads the scene from disk and can continue editing it

                var scene = ecs.FindEntitiesWithName("Scene").Single();

                Assert.Equal(3, scene.GetChildren().Count());
                var enemy1 = scene.GetChildren().ElementAt(0);
                Assert.Equal(new Vector3(1, 0, 0), enemy1.LocalPose().position);
                var enemy2 = scene.GetChildren().ElementAt(1);
                Assert.NotNull(enemy2.GetComponentInChildren<Entity, ShieldComponent>());

                {
                    // Destroying / deleting templates causes variants to inherit from the
                    // parent template (or changing to not be a variant anymore):
                    Assert.True(enemy1.IsVariant());
                    Assert.False(enemy1.IsTemplate());
                    Assert.True(enemy2.IsVariant());
                    Assert.False(enemy2.IsTemplate());
                    Assert.True(enemy1.TryGetTemplate(out var enemyTemplate));
                    {
                        Assert.True(enemyTemplate.IsTemplate());
                        Assert.Equal("EnemyTemplate", enemyTemplate.Name);
                        Assert.True(enemyTemplate.Ecs.TryGetVariants(enemyTemplate.Id, out var variants1));
                        Assert.Equal(3, variants1.Count());
                    }

                    Assert.True(enemy2.TryGetTemplate(out var bossEnemy));
                    Assert.True(enemyTemplate.IsTemplate());
                    Assert.False(enemyTemplate.IsVariant());
                    Assert.True(bossEnemy.IsTemplate());
                    { // Template 2 (bossEnemy) is a variant itself (of baseEnemy):
                        Assert.True(bossEnemy.IsVariant());
                        Assert.Equal("BossEnemy", bossEnemy.Name);
                        Assert.True(bossEnemy.TryGetTemplate(out var bossEnemyTemplate));
                        Assert.Same(enemyTemplate, bossEnemyTemplate);
                    }
                    Assert.True(await bossEnemy.Destroy());
                    Assert.True(bossEnemy.IsDestroyed());
                    // The variant of the destroyed template is now based on the parent template:
                    Assert.False(enemy2.IsDestroyed());
                    Assert.True(enemy2.IsVariant());
                    Assert.True(enemy2.TryGetTemplate(out var newTemplate2));
                    Assert.NotEqual(bossEnemy.Id, newTemplate2.Id);
                    Assert.Equal("EnemyTemplate", newTemplate2.Name);

                    {
                        Assert.True(enemyTemplate.IsTemplate());
                        Assert.True(enemyTemplate.Ecs.TryGetVariants(enemyTemplate.Id, out var variants1));
                        Assert.Equal(2, variants1.Count());

                    }
                    Assert.True(await enemyTemplate.Destroy());
                    await enemyTemplate.SaveChanges();
                    Assert.True(enemyTemplate.IsDestroyed());
                    Assert.False(enemy1.IsDestroyed());
                    // Since template1 did not have a parent template enemy1 is no longer a variant:
                    enemy1.TryGetTemplate(out var newTemplate1);
                    Assert.Equal(null, newTemplate1);
                    Assert.False(enemy1.IsVariant());

                    // Destroying a variant does not affect the parent template:
                    var firstChildInBossEnemyVariant = enemy2.GetChildren().First();
                    Assert.True(await firstChildInBossEnemyVariant.Destroy());
                    await firstChildInBossEnemyVariant.SaveChanges();
                    Assert.True(firstChildInBossEnemyVariant.IsDestroyed());
                    Assert.DoesNotContain(firstChildInBossEnemyVariant, enemy2.GetChildren());
                    var firstChildInBossEnemyTemplate = bossEnemy.GetChildren().First();
                    Assert.False(firstChildInBossEnemyTemplate.IsDestroyed());
                    Assert.Contains(firstChildInBossEnemyTemplate, bossEnemy.GetChildren());

                }
            }
        }

        private IReadOnlyDictionary<string, IComponentData> CreateComponents(IComponentData component) {
            component.GetId().ThrowErrorIfNullOrEmpty("component.GetId()");
            var dict = new Dictionary<string, IComponentData>();
            dict.Add(component.GetId(), component);
            return dict;
        }

        private void Assert_AlmostEqual(Vector3 expected, Vector3 actual, float allowedDelta = 0.000001f) {
            var distance = (expected - actual).Length();
            Assert.True(distance < allowedDelta, $"Expected {expected} but was {actual} (distance={distance}>{allowedDelta})");
        }

        private void Assert_AlmostEqual(Quaternion expected, Quaternion actual, double allowedDelta = 0.1) {
            var angle = expected.GetRotationDeltaInDegreeTo(actual);
            Assert.True(angle < allowedDelta, $"Expected {expected} but was {actual} (angle={angle}>{allowedDelta})");
        }

        private void Assert_AlmostEqual(Matrix4x4 expected, Matrix4x4 actual, float allowedDelta = 0.000001f) {
            // Decompose the matrices into their components and compare them:
            expected.Decompose(out var scaleExp, out var rotExp, out var posExp);
            actual.Decompose(out var scaleActual, out var rotActual, out var posActual);
            Assert_AlmostEqual(scaleExp, scaleActual, allowedDelta);
            Assert_AlmostEqual(rotExp, rotActual, allowedDelta);
            Assert_AlmostEqual(posExp, posActual, allowedDelta);
        }

        private abstract class Component : IComponentData {
            public string Id { get; set; } = "" + GuidV2.NewGuid();
            public string GetId() { return Id; }
            public bool IsActive { get; set; } = true;
        }

        private class EnemyComponent : Component {
            public int Mana { get; set; }
            public int Health;
        }

        private class SwordComponent : Component {
            public int Damage { get; set; }
        }

        private class ShieldComponent : Component {
            public int Defense { get; set; }
        }

    }

    public class Entity : IEntityData {
        public string Id { get; set; } = "" + GuidV2.NewGuid();
        public string Name { get; set; }
        public string TemplateId { get; set; }
        public Matrix4x4? LocalPose { get; set; }
        public IReadOnlyDictionary<string, IComponentData> Components { get; set; }
        public bool IsActive { get; set; } = true;
        public string ParentId { get; set; }
        public IReadOnlyList<string> ChildrenIds => MutablehildrenIds;
        [JsonIgnore] // Dont include the children ids two times
        public List<string> MutablehildrenIds { get; } = new List<string>();
        public string GetId() { return Id; }
        public override string ToString() { return $"{Name} ({Id})"; }
    }

    public static class EntityExtensions {

        public static IEntity<Entity> AddChild(this IEntity<Entity> parent, Entity childData) {
            return parent.AddChild(childData, SetParentIdInChild, AddToChildrenListOfParent);
        }

        public static IEntity<Entity> AddChild(this IEntity<Entity> parent, IEntity<Entity> childData) {
            return parent.AddChild(childData, SetParentIdInChild, AddToChildrenListOfParent);
        }

        public static void RemoveFromParent(this IEntity<Entity> child) {
            child.RemoveFromParent(RemoveParentIdFromChild, RemoveChildIdFromParent);
        }

        public static Task<bool> Destroy(this IEntity<Entity> self) {
            return self.Destroy(RemoveChildIdFromParent, ChangeTemplate);
        }

        private static Entity SetParentIdInChild(IEntity<Entity> child, string newParentId) {
            child.Data.ParentId = newParentId;
            return child.Data;
        }

        private static Entity AddToChildrenListOfParent(IEntity<Entity> parent, string addedChildId) {
            parent.Data.MutablehildrenIds.Add(addedChildId);
            return parent.Data;
        }

        private static Entity RemoveParentIdFromChild(IEntity<Entity> child) {
            child.Data.ParentId = null;
            return child.Data;
        }

        private static Entity RemoveChildIdFromParent(IEntity<Entity> parent, string childIdToRemove) {
            parent.Data.MutablehildrenIds.Remove(childIdToRemove);
            return parent.Data;
        }

        private static Entity ChangeTemplate(IEntity<Entity> entity, string newTemplateId) {
            entity.Data.TemplateId = newTemplateId;
            return entity.Data;
        }

        public static void SetActiveSelf(this IEntity<Entity> self, bool active) {
            self.SetActiveSelf(active, SetActiveInternal);
        }

        private static Entity SetActiveInternal(IEntity<Entity> entity, bool active) {
            entity.Data.IsActive = active;
            return entity.Data;
        }

    }

}