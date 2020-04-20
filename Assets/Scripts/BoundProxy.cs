using Unity.Entities;
using UnityEngine;

namespace BrickPlayground {
  public class BoundProxy : MonoBehaviour, IConvertGameObjectToEntity {
    public void Convert(
      Entity entity,
      EntityManager entityManager,
      GameObjectConversionSystem conversionSystem
    ) {
      entityManager.AddComponentData(entity, new BoundData());
    }
  }
}
