using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using com.csutil.json;
using com.csutil.model.ecs;
using Xunit;
using Zio;

namespace com.csutil.tests.model.esc {

    public class EntityComponentSystemTests {

        public EntityComponentSystemTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            var rootDir = EnvironmentV2.instance.GetOrAddTempFolder("EntityComponentSystemTests_ExampleUsage1");
            var entitiesDir = rootDir.GetChildDir("Entities");
            entitiesDir.DeleteV2();
            entitiesDir.CreateV2();

            var ecs = new EntityComponentSystem<Entity>(entitiesDir);

            var enemyTemplate = new Entity() {
                Id = "" + GuidV2.NewGuid(),
                LocalPose = Matrix4x4.CreateTranslation(1, 2, 3),
                Components = new List<IComponentData>() {
                    new EnemyComp() { Id = "c1", Health = 100, Mana = 10 }
                }
            };

            ecs.Save(enemyTemplate);

            // An instance that has a different health value than the template:
            Entity variant1 = ecs.CreateVariantOf(enemyTemplate);
            (variant1.Components.Single() as EnemyComp).Health = 200;
            ecs.Save(variant1);

            // Create a variant2 of the variant1
            Entity variant2 = ecs.CreateVariantOf(variant1);
            (variant2.Components.Single() as EnemyComp).Mana = 20;
            ecs.Save(variant2);

            // Updating variant 1 should also update variant2:
            (variant1.Components.Single() as EnemyComp).Health = 300;
            ecs.Save(variant1);
            variant2 = await ecs.Load(variant2.Id);
            Assert.Equal(300, (variant2.Components.Single() as EnemyComp).Health);

            // Another instance that is identical to the template:
            Entity variant3 = ecs.CreateVariantOf(enemyTemplate);
            ecs.Save(variant3);

            var ecs2 = new EntityComponentSystem<Entity>(entitiesDir);
            await ecs2.LoadAllJTokens();
            var ids = ecs2.GetAllEntityIds().ToList();
            Assert.Equal(4, ids.Count());
            Entity v1 = await ecs2.Load(variant1.Id);
            var enemyComp1 = v1.Components.Single() as EnemyComp;
            Assert.Equal(200, enemyComp1.Health);
            Assert.Equal(10, enemyComp1.Mana);

            Entity v2 = await ecs2.Load(variant2.Id);
            var enemyComp2 = v2.Components.Single() as EnemyComp;
            Assert.Equal(200, enemyComp2.Health);
            Assert.Equal(20, enemyComp2.Mana);

            entitiesDir.OpenInExternalApp();

        }

        [Fact]
        public async Task ExampleUsage2() {
            // Composing full scene graphs by using the ChildrenIds property:

            var entitiesDir = EnvironmentV2.instance.GetNewInMemorySystem();
            var ecs = new EntityComponentSystem<Entity>(entitiesDir);

            var enemyTemplate = new Entity() {
                Id = "" + GuidV2.NewGuid(),
                LocalPose = Matrix4x4.CreateTranslation(1, 2, 3),
                Components = new List<IComponentData>() {
                    new EnemyComp() { Id = "c1", Health = 100 }
                }
            };

        }

        private class Entity : IEntityData {
            public string Id { get; set; }
            public string TemplateId { get; set; }
            public Matrix4x4? LocalPose { get; set; }
            public IList<IComponentData> Components { get; set; }
            public IList<string> ChildrenIds { get; set; }
            public IList<string> Tags { get; set; }

            public string GetId() { return Id; }
        }

        private class EnemyComp : IComponentData {
            public string Id { get; set; }
            public int Mana { get; set; }
            public int Health;
            public string GetId() { return Id; }
        }

    }

}