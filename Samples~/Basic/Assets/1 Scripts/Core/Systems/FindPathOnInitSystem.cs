using DesertImage.ECS;
using Game.Navigation;
using Unity.Burst;
using UnityEngine;

namespace Game
{
    [BurstCompile]
    public struct FindPathOnInitSystem : IInitialize
    {
        public void Initialize(in World world)
        {
            var group = Filter.Create(world)
                .With<PathfindingActor>()
                .With<GridPosition>()
                .Find();

            foreach (var entityId in group)
            {
                var entity = group.GetEntity(entityId);

                var newPosition = new Vector2Int(6, 6);

                entity.Replace(new FindPath { Value = newPosition });
            }
        }
    }
}