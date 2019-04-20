
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

        public float animationPhase;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            int index = AnimatedObjectSystem.recordMeshMaterial(mesh, material);

            var renderedObject = new RenderedObject
            {
                meshRotation = meshRotation,
                meshMaterialIndex = index
            };
            dstManager.AddComponentData(entity, renderedObject);
            
            var animatedObject = new AnimatedObject
            {
                animationSpeed = animationSpeed,
                animationPhase = animationPhase
            };
            dstManager.AddComponentData(entity, animatedObject);
        }
    }
}