using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Transcendence.AnimationInstancing
{
    public class AnimatedObjectSystem : JobComponentSystem
    {
        private static Dictionary<MeshAndMaterial, int> _meshMaterialMap = new Dictionary<MeshAndMaterial, int>();
        private static List<MeshAndMaterial> _meshMaterialList = new List<MeshAndMaterial>();
        

        public static int recordMeshMaterial(Mesh mesh, Material material)
        {
            MeshAndMaterial pair = new MeshAndMaterial {mesh = mesh, material = material};
            if (_meshMaterialMap.ContainsKey(pair))
            {
                return _meshMaterialMap[pair];
            }
            else
            {
                int index = _meshMaterialList.Count;
                _meshMaterialMap.Add(pair, index);
                _meshMaterialList.Add(pair);
                return index;
            }
        }

        [BurstCompile]
        struct DeallocateMemoryJob : IJob
        {
            [DeallocateOnJobCompletionAttribute]
            public NativeArray<int> meshMaterialCounts;
            
            [DeallocateOnJobCompletionAttribute]
            public NativeArray<AnimatedObject> animatedObjects;

            [DeallocateOnJobCompletionAttribute]
            public NativeArray<int> numberOfBatches;
            
            public void Execute()
            {
            }
        }

        [BurstCompile]
        struct CountEntitiesPerMeshMaterialJob : IJob
        {
            public NativeArray<AnimatedObject> animatedObjects;

            public NativeArray<int> meshMaterialCounts;

            public NativeArray<int> batchesPerMeshMaterial;
            
            public void Execute()
            {
                for (int i = 0; i < animatedObjects.Length; i++)
                {
                    var meshMaterialIndex = animatedObjects[i].meshMaterialIndex;
                    int newCount = meshMaterialCounts[meshMaterialIndex] + 1;
                    meshMaterialCounts[meshMaterialIndex] = newCount;
                }

                for (int i = 0; i < meshMaterialCounts.Length; i++)
                {
                    var meshMaterialCount = meshMaterialCounts[i];
                    var numBatches = meshMaterialCount / 1023 + (meshMaterialCount % 1023 > 0 ? 1 : 0);
                    batchesPerMeshMaterial[i] = numBatches;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            
            // materialMeshIndex -> batches
            //var materialMeshBatches = new NativeArray<NativeArray<AnimationInstanceBatch>>(_instanceList.Count, Allocator.TempJob);

            EntityQuery query = GetEntityQuery(typeof(AnimatedObject), typeof(LocalToWorld));
            var animatedObjects = query.ToComponentDataArray<AnimatedObject>(Allocator.TempJob);

            NativeArray<int> meshMaterialCounts = new NativeArray<int>(_meshMaterialList.Count, Allocator.TempJob);
            NativeArray<int> batchesPerMeshMaterial = new NativeArray<int>(_meshMaterialList.Count, Allocator.TempJob);

            var countEntitiesPerMeshMaterialJob = new CountEntitiesPerMeshMaterialJob
            {
                animatedObjects = animatedObjects,
                meshMaterialCounts = meshMaterialCounts,
                batchesPerMeshMaterial = batchesPerMeshMaterial
            };
            countEntitiesPerMeshMaterialJob.Execute();
            
            
            
            
            return inputDeps;




//            AnimationInstanceBatchRecorder recorder = new AnimationInstanceBatchRecorder();
//            Entities.ForEach((ref LocalToWorld localToWorld, ref AnimationInstance animationInstance) =>
//            {
//                float4x4 transformation =
//                    math.mul(localToWorld.Value, float4x4.EulerXYZ(math.radians(animationInstance.meshRotation)));
//                
//                MeshAndMaterial meshAndMaterial = _instanceList[animationInstance.instanceIndex];
//                recorder.addBatch(meshAndMaterial,
//                    transformation,
//                    new Vector4(0, 0, animationInstance.animationSpeed, 1),
//                    animationInstance.animationPhase
//                );
//            });
//
//            foreach (var batchList in recorder.Batches)
//            {
//                foreach (var batch in batchList.Value)
//                {
//                    MaterialPropertyBlock block = new MaterialPropertyBlock();
//                    block.SetVectorArray("_Anim", batch.animationSettings);
//                    block.SetFloatArray("_AnimPhase", batch.animationPhases);
//
//                    Graphics.DrawMeshInstanced(batchList.Key.mesh, 0, batchList.Key.material, batch.transformations,
//                        block);
//                }
//            }
        }
    }
}