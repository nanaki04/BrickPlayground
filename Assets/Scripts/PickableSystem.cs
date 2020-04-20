using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BrickPlayground {

  public class PickableSystem : JobComponentSystem {
    BuildPhysicsWorld physicsWorld;
    BeginInitializationEntityCommandBufferSystem commandBufferSystem;

    struct MoveInput {
      public UnityEngine.Ray Ray;
      public int TouchId;
    }

    struct TouchInfo {
      public Vector2 Pos;
      public int TouchId;
    }

    protected override void OnCreate()
    {
      physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
      commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle handle)
    {
      handle = UpdateEndDrag(handle);
      handle = Drag(handle);

      return handle;
    }

    JobHandle UpdateEndDrag(JobHandle handle) {
      var dragEndIds = Input.touches
        .Where(touch => touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        .Select(touch => touch.fingerId)
        .ToList();

      if (Input.GetMouseButtonUp(0)) {
        dragEndIds.Add(-2);
      }

      if (dragEndIds.Count == 0) {
        return handle;
      }

      var endedTouchIds = new NativeArray<int>(dragEndIds.Count, Allocator.TempJob);
      for (int i = 0; i < dragEndIds.Count; i++) {
        endedTouchIds[i] = dragEndIds[i];
      }

      return Entities
        .ForEach((ref TappableData tappableData) => {
          for (int i = 0; i < endedTouchIds.Length; i++) {
            if (tappableData.TouchId != endedTouchIds[i]) {
              continue;
            }

            tappableData.TouchId = -1;
            tappableData.Tapped = false;
            break;
          }
        })
        .WithDeallocateOnJobCompletion(endedTouchIds)
        .Schedule(handle);
    }

    JobHandle Drag(JobHandle handle) {
      var movedTouches = Input.touches
        .Where(touch => touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
        .Select(touch => new TouchInfo {
            Pos = touch.position,
            TouchId = touch.fingerId,
        })
        .ToList();

      movedTouches.Add(new TouchInfo {
        Pos = Input.mousePosition,
        TouchId = -2,
      });

      var inputList = new NativeArray<MoveInput>(movedTouches.Count, Allocator.TempJob);
      for (int i = 0; i < movedTouches.Count; i++) {
        var point = movedTouches[i].Pos;
        var ray = Camera.main.ScreenPointToRay(point);
        inputList[i] = new MoveInput {
          Ray = ray,
          TouchId = movedTouches[i].TouchId,
        };
      }

      return Entities
        .ForEach((ref Translation translation, in TappableData tappableData) => {
          if (!tappableData.Tapped) {
            return;
          }

          for (int i = 0; i < inputList.Length; i++) {
            if (inputList[i].TouchId != tappableData.TouchId) {
              continue;
            }

            var ray = inputList[i].Ray;
            var newPoint = ray.GetPoint(8f);

            translation.Value = new float3(newPoint.x, newPoint.y, newPoint.z);
            break;
          }
        })
        .WithDeallocateOnJobCompletion(inputList)
        .Schedule(handle);
    }
  }
}
