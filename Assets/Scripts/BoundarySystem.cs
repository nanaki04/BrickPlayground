using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;

namespace BrickPlayground {

  public class BoundarySystem : JobComponentSystem {

    const float minZ = -8f;
    const float maxZ = 8f;
    const float minY = 0f;
    const float maxY = 8f;
    const float limitXminusZ = 10f;

    protected override JobHandle OnUpdate(JobHandle handle) {
      return Entities
        .ForEach((ref Translation translation, in BoundData boundData) => {
          var maxX = limitXminusZ + translation.Value.z;
          var minX = -1 * maxX;

          if (translation.Value.x < minX) translation.Value.x = minX;
          if (translation.Value.x > maxX) translation.Value.x = maxX;
          if (translation.Value.y < minY) translation.Value.y = minY;
          if (translation.Value.y > maxY) translation.Value.y = maxY;
          if (translation.Value.z < minZ) translation.Value.z = minZ;
          if (translation.Value.z > maxZ) translation.Value.z = maxZ;
        })
        .Schedule(handle);
    }

  }

}
