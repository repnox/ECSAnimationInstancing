using Unity.Entities;
using UnityEngine;

namespace Transcendence.ECS.Movement
{
    [RequiresEntityConversion]
    public class ForwardMovementProxy : MonoBehaviour, IConvertGameObjectToEntity
    {

        public float speed;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new ForwardMovement{MoveSpeed = speed};
            dstManager.AddComponentData(entity, data);
        }
    }
}