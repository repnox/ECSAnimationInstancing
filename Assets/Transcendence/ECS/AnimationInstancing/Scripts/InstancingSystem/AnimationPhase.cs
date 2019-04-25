using System;
using Unity.Entities;

namespace Transcendence.AnimationInstancing
{
    [Serializable]
    public struct AnimationPhase : IComponentData
    {
        public float value;
    }
}