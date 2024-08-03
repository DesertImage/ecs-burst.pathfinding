using DesertImage.ECS;
using Game.Navigation;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Game
{
    [BurstCompile]
    public struct WallsSpawnSystem : IInitialize
    {
        public void Initialize(in World world)
        {
            for (var i = 0; i < 3; i++)
            {
                var entity = world.GetNewEntity();

                entity.Replace(new GridPosition { Value = new Vector2Int(2 + i, 4) });
                entity.Replace<Position>();

                entity.InstantiateView(1, new float3(0f, 0f, 0f));
            }
        }
    }
}