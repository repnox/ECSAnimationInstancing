using System;
using Unity.Mathematics;
using UnityEngine;

namespace Transcendence.AnimationInstancing
{
    [Serializable]
    public struct AnimationSettings
    {
        
        public int textureIndex;

        public float2 positionOffsetPixels;

        public float animationSpeed;

        public float animationDurationRatio;
    }
}