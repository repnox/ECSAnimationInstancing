using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Transcendence.AnimationInstancing
{
    [Serializable]
    public struct AnimatedObject : IComponentData
    {
        public AnimationSettings animationSettings;
    }
}