
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Transcendence.ECS.Movement
{
    public class ForwardMovementSystem : JobComponentSystem
    {

        [BurstCompile]
        struct ForwardMovementJob : IJobForEach<Rotation, Translation, ForwardMovement>
        {
            public float DeltaTime;
            
            public void Execute([ReadOnly] ref Rotation rotation, ref Translation translation,[ReadOnly] ref ForwardMovement forwardMovement)
            {
                translation.Value += math.forward(rotation.Value) * forwardMovement.MoveSpeed;
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new ForwardMovementJob()
            {
                DeltaTime = Time.deltaTime
            };
    
            return job.Schedule(this, inputDependencies);
        }
    }
}