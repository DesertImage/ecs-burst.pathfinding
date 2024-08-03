using DesertImage.Collections;
using DesertImage.ECS;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace Game.Navigation
{
    /// <summary>
    /// Initialize Pathfinding data accordingly to Map data
    /// </summary>
    [BurstCompile]
    public struct PathfindingInitializeSystem : IInitialize, IDestroy
    {
        public void Initialize(in World world)
        {
            ref var pathfinding = ref world.GetStatic<Pathfinding>();
            var map = world.GetStatic<Map.Map>();

            var mapSize = map.Width * map.Height;

            var mapWidth = map.Width;

            pathfinding.NodesWalkableState = new UnsafeArray<byte>(mapSize, Allocator.Persistent, Pathfinding.WALKABLE);
            pathfinding.MapWidth = mapWidth;
            pathfinding.MapHeight = map.Height;

            for (var i = 0; i < map.CellsFreeState.Length; i++)
            {
                pathfinding.NodesWalkableState[i] = map.CellsFreeState[i];
            }
            
            var initGroup = Filter.Create(world)
                .With<PathfindingActor>()
                .Find();

            foreach (var entityId in initGroup)
            {
                var entity = initGroup.GetEntity(entityId);
                ref var actor = ref entity.Get<PathfindingActor>();

                actor.Nodes = new UnsafeArray<PathNode>(mapSize, Allocator.Persistent);
                for (var i = 0; i < mapWidth; i++)
                {
                    for (var j = 0; j < map.Height; j++)
                    {
                        var index = i + j * mapWidth;

                        actor.Nodes[index] = new PathNode
                        {
                            Index = index,
                            GridPosition = new Vector2Int(i, j),
                        };
                    }
                }
            }
        }

        public void OnDestroy(in World world)
        {
            var group = Filter.Create(world)
                .With<PathfindingActor>()
                .Find();

            var actorsList = group.GetComponents<PathfindingActor>();
            
            foreach (var entityId in group)
            {
                var actor = actorsList.Read(entityId);
                actor.Nodes.Dispose();
                actor.Path.Dispose();
            }
        }
    }
}