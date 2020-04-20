using Unity.Entities;
using UnityEngine;

namespace BrickPlayground {
  public class TappableProxy : MonoBehaviour, IConvertGameObjectToEntity {
    public void Convert(
      Entity entity,
      EntityManager entityManager,
      GameObjectConversionSystem conversionSystem
    ) {
      entityManager.AddComponentData(entity, new TappableData() {
        Tapped = false,
        TouchId = -1,
      });
    }
  }
}
