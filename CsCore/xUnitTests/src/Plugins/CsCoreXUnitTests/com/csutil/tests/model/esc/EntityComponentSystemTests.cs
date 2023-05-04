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
                    new EnemyComp() { Id = "c1", Health = 100 }
                }
            };

            ecs.Save(enemyTemplate);

            // An instance that has a different health value than the template:
            Entity variant1 = ecs.CreateVariantOf(enemyTemplate);
            (variant1.Components.Single() as EnemyComp).Health = 200;
            ecs.Save(variant1);

            // Create a variant2 of the variant1
            Entity variant2 = ecs.CreateVariantOf(variant1);
            (variant2.Components.Single() as EnemyComp).Health = 300;
            ecs.Save(variant2);

            // Another instance that is identical to the template:
            Entity variant3 = ecs.CreateVariantOf(enemyTemplate);
            ecs.Save(variant3);


            var ecs2 = new EntityComponentSystem<Entity>(entitiesDir);
            await ecs2.LoadAllJTokens();
            var ids = ecs2.GetAllEntityIds().ToList();
            Assert.Equal(4, ids.Count());
            Entity v1 = await ecs2.Load(variant1.Id);
            Assert.Equal(200, (v1.Components.Single() as EnemyComp).Health);

            Entity v2 = await ecs2.Load(variant2.Id);
            Assert.Equal(300, (v2.Components.Single() as EnemyComp).Health);

        }

        private class Entity : IEntityData {
            public string Id { get; set; }
            public string TemplateId { get; }
            public Matrix4x4? LocalPose { get; set; }
            public IList<IComponentData> Components { get; set; }
            public IList<string> ChildrenIds { get; set; }
            public IList<string> Tags { get; set; }

            public string GetId() { return Id; }
        }

        private class EnemyComp : IComponentData {
            public string Id { get; set; }
            public int Health;
            public string GetId() { return Id; }
        }

    }

}