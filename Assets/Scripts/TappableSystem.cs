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

  public class TappableSystem : JobComponentSystem {
    BuildPhysicsWorld physicsWorld;
    BeginInitializationEntityCommandBufferSystem commandBufferSystem;

    struct ScanInput {
      public RaycastInput RaycastInput;
      public int TouchId;
    }

    struct TouchInfo {
      public Vector2 Pos;
      public int TouchId;
    }

    protected override void OnCreate() {
      physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
      commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle handle) {
      var touches = Input.touches
        .Where(touch => touch.phase == TouchPhase.Began)
        .Select(touch => new TouchInfo {
            Pos = touch.position,
            TouchId = touch.fingerId,
        })
        .ToList();

      if (Input.GetMouseButtonDown(0)) {
        touches.Add(new TouchInfo {
          Pos = Input.mousePosition,
          TouchId = -2,
        });
      }

      if (touches.Count == 0) {
        return handle;
      }

      var commandBuffer = commandBufferSystem.CreateCommandBuffer();

      var filter = CollisionFilter.Default;
      filter.CollidesWith = 1u << 1;

      var rays = new NativeArray<ScanInput>(touches.Count, Allocator.TempJob);
      for (int i = 0; i < touches.Count; i++) {
        var dragStartPoint = touches[i];
        var ray = Camera.main.ScreenPointToRay(dragStartPoint.Pos);
        var raycastInput = new RaycastInput {
          Start = ray.origin,
          End = ray.origin + ray.direction * 100f,
          Filter = filter,
        };
        rays[i] = new ScanInput {
          RaycastInput = raycastInput,
          TouchId = dragStartPoint.TouchId,
        };
      }

      var collisionWorld = physicsWorld.PhysicsWorld.CollisionWorld;

      handle = JobHandle.CombineDependencies(handle, physicsWorld.FinalJobHandle);

      handle = Job
        .WithCode(() => {
          for (int i = 0; i < rays.Length; i++) {
            Unity.Physics.RaycastHit hit;
            var scanInput = rays[i];
            if (!collisionWorld.CastRay(scanInput.RaycastInput, out hit)) {
              continue;
            };

            var entity = hit.Entity;
            commandBuffer.SetComponent(entity, new TappableData {
              Tapped = true,
              TouchId = scanInput.TouchId,
            });
          }
        })
        .WithDeallocateOnJobCompletion(rays)
        .Schedule(handle);

      handle.Complete();

      return handle;
    }
  }
}
