using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Transcendence.AnimationInstancing
{
    public class AnimatedObjectSystem : ComponentSystem
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
        struct CountBatchesJob : IJob
        {
            
            public NativeArray<RenderedObject> renderedObjects;

            public NativeArray<int> meshMaterialChunkOffsets;

            public NativeArray<int> totalBatchesContainer;

            public NativeArray<int> meshMaterialCounts;
            
            public void Execute()
            {

                // Count the number of objects per MeshMaterial
                for (int i = 0; i < renderedObjects.Length; i++)
                {
                    var meshMaterialIndex = renderedObjects[i].meshMaterialIndex;
                    int newCount = meshMaterialCounts[meshMaterialIndex] + 1;
                    meshMaterialCounts[meshMaterialIndex] = newCount;
                }

                // Determine how many batches will exist for each MeshMaterial.
                int totalBatches = 0;
                int offset = 0;
                // NativeArray<int> meshMaterialChunkOffsets = new NativeArray<int>(meshMaterialCounts.Length, Allocator.Temp);
                for (int i = 0; i < meshMaterialCounts.Length; i++)
                {
                    var meshMaterialCount = meshMaterialCounts[i];
                    var numBatches = meshMaterialCount / 1023 + (meshMaterialCount % 1023 > 0 ? 1 : 0);
                    meshMaterialChunkOffsets[i] = offset;
                    
                    offset += numBatches;

                    var newTotalBatches = totalBatches + numBatches;
                    totalBatches = newTotalBatches;
                }

                totalBatchesContainer[0] = totalBatches;
            }
        }

        [BurstCompile]
        struct AssignToBatchesJob : IJob
        {
            public NativeArray<int> entityCounts;
            
            public NativeMultiHashMap<int, int> chunkIndexMap;
            
            public NativeArray<RenderedObject> renderedObjects;
            
            public NativeArray<int> meshMaterialChunkOffsets;
            
            public NativeArray<int> batchMeshMaterials;
            
            public void Execute() {
                
                // Determine which chunk each object belongs to.
                //NativeArray<int> entityCounts = new NativeArray<int>(totalBatchesContainer[0], Allocator.Temp);
                for (int i = 0; i < renderedObjects.Length; i++)
                {
                    var meshMaterial = renderedObjects[i].meshMaterialIndex;
                    var chunkOffset = meshMaterialChunkOffsets[meshMaterial];
                    for (int chunk = chunkOffset; chunk < entityCounts.Length; chunk++)
                    {
                        if (entityCounts[chunk] < 1023)
                        {
                            batchMeshMaterials[chunk] = meshMaterial;
                            chunkIndexMap.Add(chunk, i);
                            var newCount = entityCounts[chunk] + 1;
                            entityCounts[chunk] = newCount;
                            break;
                        }
                    }
                }
                
            }
        }

        [BurstCompile]
        struct CalculateAnimationDataJob : IJobForEachWithEntity<AnimatedObject, RenderedObject, LocalToWorld>
        {
            
            public NativeArray<float4x4> transformations;
            public NativeArray<float4> animationSettings;
            public NativeArray<float> animationPhases;

            public void Execute(Entity entity, int index, ref AnimatedObject animatedObject, ref RenderedObject renderedObject, ref LocalToWorld localToWorld)
            {
                transformations[index] = math.mul(localToWorld.Value, 
                    float4x4.EulerXYZ(math.radians(renderedObject.meshRotation)));
                animationSettings[index] = new float4(0, 0, animatedObject.animationSpeed, 1);
                animationPhases[index] = animatedObject.animationPhase;
            }
        }

        protected override void OnUpdate()
        {

            EntityQuery query = GetEntityQuery(typeof(AnimatedObject), typeof(RenderedObject), typeof(LocalToWorld));
            var renderedObjects = query.ToComponentDataArray<RenderedObject>(Allocator.TempJob);

            CalculateAnimationDataJob calculateAnimationDataJob = new CalculateAnimationDataJob
            {
                transformations = new NativeArray<float4x4>(renderedObjects.Length, Allocator.TempJob),
                animationPhases = new NativeArray<float>(renderedObjects.Length, Allocator.TempJob),
                animationSettings = new NativeArray<float4>(renderedObjects.Length, Allocator.TempJob)
            };
            var calculateAnimationDataJobHandle = calculateAnimationDataJob.Schedule(this);

            CountBatchesJob countBatchesJob = new CountBatchesJob
            {
                renderedObjects = renderedObjects,
                totalBatchesContainer = new NativeArray<int>(1, Allocator.TempJob),
                meshMaterialChunkOffsets = new NativeArray<int>(_meshMaterialList.Count, Allocator.TempJob),
                meshMaterialCounts = new NativeArray<int>(_meshMaterialList.Count, Allocator.TempJob)
            };
            var countBatchesJobHandle = countBatchesJob.Schedule();
            countBatchesJobHandle.Complete();

            var totalBatches = countBatchesJob.totalBatchesContainer[0];
            NativeMultiHashMap<int, int> chunkIndexMap = new NativeMultiHashMap<int, int>(totalBatches, Allocator.TempJob);
            NativeArray<int> batchMeshMaterials = new NativeArray<int>(totalBatches, Allocator.TempJob);
            AssignToBatchesJob assignToBatchesJob = new AssignToBatchesJob
            {
                entityCounts = new NativeArray<int>(totalBatches, Allocator.TempJob),
                chunkIndexMap = chunkIndexMap,
                renderedObjects = renderedObjects,
                meshMaterialChunkOffsets = countBatchesJob.meshMaterialChunkOffsets,
                batchMeshMaterials = batchMeshMaterials
            };
            var assignToBatchesJobHandle = assignToBatchesJob.Schedule();
            assignToBatchesJobHandle.Complete();
            
            calculateAnimationDataJobHandle.Complete();

            for (int i = 0; i < countBatchesJob.totalBatchesContainer[0]; i++)
            {
                int meshMaterialIndex = assignToBatchesJob.batchMeshMaterials[i];
                MeshAndMaterial meshMaterial = _meshMaterialList[meshMaterialIndex];
                
                var entityCount = assignToBatchesJob.entityCounts[i];
                var batch = new AnimationDrawBatch
                {
                    matrices = new Matrix4x4[entityCount],
                    animationPhases = new float[entityCount],
                    animationSettings = new Vector4[entityCount],
                    meshAndMaterial = meshMaterial
                };
                
                int entityIndex;
                int batchInternalIndex = 0;
                NativeMultiHashMapIterator<int> iterator;
                if (chunkIndexMap.TryGetFirstValue(i, out entityIndex, out iterator))
                {
                    do
                    {
                        batch.animationPhases[batchInternalIndex] = calculateAnimationDataJob.animationPhases[entityIndex];
                        batch.animationSettings[batchInternalIndex] =
                            calculateAnimationDataJob.animationSettings[entityIndex];
                        batch.matrices[batchInternalIndex] = calculateAnimationDataJob.transformations[entityIndex];
                        
                        batchInternalIndex++;
                    } while (chunkIndexMap.TryGetNextValue(out entityIndex, ref iterator));
                }
                
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetVectorArray("_Anim", batch.animationSettings);
                block.SetFloatArray("_AnimPhase", batch.animationPhases);
                Graphics.DrawMeshInstanced(batch.meshAndMaterial.mesh, 0, batch.meshAndMaterial.material,
                    batch.matrices, batch.matrices.Length, block);
            }

            calculateAnimationDataJob.transformations.Dispose();
            calculateAnimationDataJob.animationPhases.Dispose();
            calculateAnimationDataJob.animationSettings.Dispose();
            
            countBatchesJob.totalBatchesContainer.Dispose();
            countBatchesJob.meshMaterialChunkOffsets.Dispose();
            countBatchesJob.meshMaterialCounts.Dispose();
            
            assignToBatchesJob.entityCounts.Dispose();
            assignToBatchesJob.chunkIndexMap.Dispose();
            
            batchMeshMaterials.Dispose();
            renderedObjects.Dispose();
        }

        public struct AnimationDrawBatch
        {
            public MeshAndMaterial meshAndMaterial;
            public Matrix4x4[] matrices;
            public Vector4[] animationSettings;
            public float[] animationPhases;
            
        }
        
    }
}