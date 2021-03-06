
using Unity.Entities;
using UnityEngine;

class ConvertMarine : MonoBehaviour, IConvertGameObjectToEntity
{

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        conversionSystem.AddHybridComponent(GetComponent<SpriteRenderer>());
        conversionSystem.AddHybridComponent(GetComponent<Animator>());
    }
}