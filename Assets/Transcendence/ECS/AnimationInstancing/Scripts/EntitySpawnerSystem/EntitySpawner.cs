using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


namespace Transcendence.ECS.Util
{
    [Serializable]
    public struct EntitySpawner : IComponentData
    {
        public Entity prefab;
    
        public int numX;

        public int numZ;

        public float gap;


    }


}