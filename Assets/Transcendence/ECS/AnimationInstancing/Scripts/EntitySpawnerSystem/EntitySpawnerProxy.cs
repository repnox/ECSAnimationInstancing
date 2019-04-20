using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Transcendence.ECS.Util
{
    [RequiresEntityConversion]
    public class EntitySpawnerProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        public GameObject prefab;
    
        public int numX;

        public int numZ;

        public float gap;
        
        public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
        {
            gameObjects.Add(prefab);
        }
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            EntitySpawner entitySpawner = new EntitySpawner
            {
                prefab = conversionSystem.GetPrimaryEntity(prefab),
                numX = numX,
                numZ = numZ,
                gap = gap
            };
            
            dstManager.AddComponentData(entity, entitySpawner);
        }
    }
}