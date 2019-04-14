using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Transcendence.AnimationInstancing
{
    [Serializable]
    public struct AnimatedObject : IComponentData
    {
        public int meshMaterialIndex;
        public float3 meshRotation;
        public float animationSpeed;
        public float animationPhase;
    }
}