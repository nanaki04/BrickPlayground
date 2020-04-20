using Unity.Entities;
using UnityEngine;

namespace BrickPlayground {
  public struct TappableData : IComponentData
  {
    public bool Tapped;
    public int TouchId;
  }
}
