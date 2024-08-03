using DesertImage.Collections;
using DesertImage.ECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Navigation
{
    public struct AStarPathfindingSystem : IInitialize, IExecute, IDestroy
    {
        private EntitiesGroup _group;

        public void Initialize(in World world)
        {
            _group = Filter.Create(world)
                .With<GridPosition>()
                .With<NavigationActor>()
                .With<FindPath>()
                .Find();
        }

        public void Execute(ref SystemsContext context)
        {
            var pathfinding = context.World.GetStatic<Pathfinding>();

            var job = new PathfindingJob
            {
                Entities = _group.Values,
                GridPositionsList = _group.GetComponents<GridPosition>(),
                NavActorsList = _group.GetComponents<NavigationActor>(),
                FindPathList = _group.GetComponents<FindPath>(),
                NodesWalkableState = pathfinding.NodesWalkableState,
                MapWidth = pathfinding.MapWidth,
                MapHeight = pathfinding.MapHeight
            };

            context.Handle = job.Schedule(context.Handle);
        }

        public void OnDestroy(in World world)
        {
            world.GetStatic<Pathfinding>().NodesWalkableState.Dispose();
        }

        [BurstCompile]
        private struct PathfindingJob : IJob
        {
            public UnsafeReadOnlyArray<uint> Entities;

            public UnsafeUintReadOnlySparseSet<GridPosition> GridPositionsList;
            public UnsafeUintReadOnlySparseSet<NavigationActor> NavActorsList;
            public UnsafeUintReadOnlySparseSet<FindPath> FindPathList;

            public UnsafeArray<byte> NodesWalkableState;

            public int MapWidth;
            public int MapHeight;

            public void Execute()
            {
                //TODO: priorityQueue instead UnsafeList
                var openSet = new UnsafeList<int>(256, Allocator.Temp);
                var closeSet = new UnsafeNoCollisionHashSet<int>(256, Allocator.Temp);

                var offsets = new UnsafeArray<Vector2Int>(4, Allocator.Temp);
                offsets[0] = new Vector2Int(-1, 0);
                offsets[1] = new Vector2Int(1, 0);
                offsets[2] = new Vector2Int(0, 1);
                offsets[3] = new Vector2Int(0, -1);

                foreach (var entityId in Entities)
                {
                    var GridPosition = GridPositionsList.Read(entityId).Value;
                    ref var actor = ref NavActorsList.Get(entityId);

                    var findPath = FindPathList.Read(entityId);
                    var target = findPath.Value;
                    var targetIndex = target.x + target.y * MapWidth;

                    for (var i = 0; i < actor.Nodes.Length; i++)
                    {
                        ref var node = ref actor.Nodes.Get(i);
                        node.FCost = int.MaxValue;
                        node.GCost = int.MaxValue;
                        node.HCost = int.MaxValue;
                    }

                    var startHCost = GetHDistance(GridPosition, target);
                    var startIndex = GetIndex(GridPosition, MapWidth);

                    ref var startNode = ref actor.Nodes.Get(startIndex);

                    startNode.GCost = 0;
                    startNode.HCost = startHCost;
                    startNode.FCost = startHCost;
                    startNode.Previous = -1;

                    openSet.Add(startNode.Index);

                    while (openSet.Count > 0)
                    {
                        var currentNode = actor.Nodes[openSet[0]];

                        //find cheapest FCost
                        for (var i = 0; i < openSet.Count; i++)
                        {
                            var checkNode = actor.Nodes[openSet[i]];

                            if (checkNode.FCost < currentNode.FCost ||
                                (checkNode.FCost == currentNode.FCost && checkNode.HCost < currentNode.HCost))
                            {
                                currentNode = checkNode;
                            }
                        }

                        if (currentNode.GridPosition == target)
                        {
                            actor.Path.Clear();

                            while (currentNode.Previous >= 0)
                            {
                                actor.Path.Push(currentNode.Index);
                                currentNode = actor.Nodes[currentNode.Previous];
                            }

                            for (var i = 0; i < findPath.PointOffset; i++)
                            {
                                if (actor.Path.Count == 0) break;
                                actor.Path.Pull();
                            }

                            break;
                        }

                        var currentNodeIndex = currentNode.Index;

                        for (var i = 0; i < openSet.Count; i++)
                        {
                            var index = openSet[i];
                            if (index != currentNodeIndex) continue;
                            openSet.RemoveAt(i);
                            break;
                        }

                        closeSet.Add(currentNodeIndex);

                        //Iterate through neighbours of currentNode
                        foreach (var offset in offsets)
                        {
                            var neighbourGridPosition = currentNode.GridPosition + offset;
                            var neighbourIndex = GetIndex(neighbourGridPosition, MapWidth);

                            if (neighbourGridPosition.x < 0 || neighbourGridPosition.x >= MapWidth) continue;
                            if (neighbourGridPosition.y < 0 || neighbourGridPosition.y >= MapHeight) continue;

                            if (closeSet.Contains(neighbourIndex)) continue;

                            ref var neighbour = ref actor.Nodes.Get(neighbourIndex);

                            if (NodesWalkableState[neighbourIndex] != Pathfinding.WALKABLE &&
                                neighbourIndex != targetIndex)
                            {
                                continue;
                            }

                            var newCostToNeighbour = currentNode.GCost + 1;

                            if (newCostToNeighbour >= neighbour.GCost) continue;

                            neighbour.GCost = newCostToNeighbour;
                            neighbour.HCost = GetHDistance(neighbourGridPosition, target);
                            neighbour.FCost = neighbour.GCost + neighbour.HCost;
                            neighbour.Previous = currentNode.Index;

                            if (!openSet.Contains(neighbourIndex))
                            {
                                openSet.Add(neighbourIndex);
                            }
                        }
                    }
                }

                openSet.Dispose();
                closeSet.Dispose();

                offsets.Dispose();
            }
        }

        private static int GetIndex(Vector2Int pos, int mapWidth) => pos.x + pos.y * mapWidth;

        private static int GetHDistance(Vector2Int value, Vector2Int target)
        {
            return math.abs(value.x - target.x) + math.abs(value.y - target.y);
        }
    }
}