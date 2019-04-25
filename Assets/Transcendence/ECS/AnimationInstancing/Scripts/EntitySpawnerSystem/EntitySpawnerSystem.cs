using Transcendence.AnimationInstancing;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;

namespace Transcendence.ECS.Util
{
    public class EntitySpawnerSystem : JobComponentSystem
    {
        
        struct SpawnEntitiesJob : IJobForEachWithEntity<EntitySpawner, LocalToWorld>
        {
            public EntityCommandBuffer commandBuffer;

            public void Execute(Entity entity, int index, ref EntitySpawner entitySpawner, [ReadOnly] ref LocalToWorld spawnerLocalToWorld)
            {
                Random random = new Random(346273);
                for (int x = 0; x < entitySpawner.numX; x++)
                {
                    for (int z = 0; z < entitySpawner.numZ; z++)
                    {
                        var instance = commandBuffer.Instantiate(entitySpawner.prefab);
                        
                        commandBuffer.AddComponent(instance, new AnimationPhase
                        {
                            value = random.NextFloat()
                        });
                        
                        commandBuffer.SetComponent(instance, new Rotation
                        {
                            Value = quaternion.AxisAngle(math.up(), random.NextFloat() * 2f * 3.141592654f)
                        });
                        
                        var position = math.transform(spawnerLocalToWorld.Value, new float3(x*entitySpawner.gap, 0, z*entitySpawner.gap));
                        commandBuffer.SetComponent(instance, new Translation{Value = position});
                    }
                }
                
                commandBuffer.DestroyEntity(entity);
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var entityCommandBufferSystem =
                World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            var jobHandle = new SpawnEntitiesJob
            {
                commandBuffer = entityCommandBufferSystem.CreateCommandBuffer()
            }.ScheduleSingle(this, inputDeps);
        
            entityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }
    }
}