using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Transcendence.AnimationInstancing
{
    [Serializable]
    public struct RenderedObject : IComponentData
    {
        public int meshMaterialIndex;
        public float3 meshRotation;
    }
}