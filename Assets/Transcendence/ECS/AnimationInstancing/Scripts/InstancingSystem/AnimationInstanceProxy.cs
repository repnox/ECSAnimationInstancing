
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Transcendence.AnimationInstancing
{
    [RequiresEntityConversion]
    public class AnimationInstanceProxy : MonoBehaviour, IConvertGameObjectToEntity
    {

        public Mesh mesh;

        public Vector3 meshRotation;

        public Material material;

        public float animationSpeed;

        public bool randomPhase = true;

        public float animationPhase;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            int index = AnimatedObjectSystem.recordMeshMaterial(mesh, material);
            var phase = randomPhase ? Random.Range(0f, 1f) : animationPhase;
            var data = new AnimatedObject
            {
                meshMaterialIndex = index,
                meshRotation = meshRotation,
                animationSpeed = animationSpeed,
                animationPhase = phase
            };
            dstManager.AddComponentData<AnimatedObject>(entity, data);
        }
    }
}