using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Transcendence.AnimationInstancing
{
    [Serializable]
    public struct AnimatedObject : IComponentData
    {
        public float animationSpeed;
        public float animationPhase;
    }
}